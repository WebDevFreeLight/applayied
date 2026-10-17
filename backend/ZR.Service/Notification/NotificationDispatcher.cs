using Infrastructure.Attribute;
using Microsoft.AspNetCore.SignalR;
using NLog;
using ZR.Model.Notification;
using ZR.Service.HR.IService;
using ZR.Service.Notification.IService;
using ZR.ServiceCore.Services;
using ZR.ServiceCore.Signalr;

namespace ZR.Service.Notification
{
    [AppService(ServiceType = typeof(INotificationDispatcher), ServiceLifetime = LifeTime.Transient)]
    public class NotificationDispatcher : INotificationDispatcher
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IErpNotificationService _notificationService;
        private readonly ISysRoleService _roleService;
        private readonly ISysUserRoleService _userRoleService;
        private readonly IHrEmployeeCoreService _employeeService;
        private readonly IHubContext<MessageHub> _hubContext;

        public NotificationDispatcher(
            IErpNotificationService notificationService,
            ISysRoleService roleService,
            ISysUserRoleService userRoleService,
            IHrEmployeeCoreService employeeService,
            IHubContext<MessageHub> hubContext)
        {
            _notificationService = notificationService;
            _roleService = roleService;
            _userRoleService = userRoleService;
            _employeeService = employeeService;
            _hubContext = hubContext;
        }

        public async Task SendAsync(NotificationRequest request)
        {
            Console.WriteLine($"[NotificationDispatcher] SendAsync called. RoleKeys={string.Join(",", request.RoleKeys ?? new List<string>())}, UserIds={string.Join(",", request.UserIds ?? new List<long>())}, EmployeeIds={string.Join(",", request.EmployeeIds ?? new List<long>())}");
            var targetUserIds = new HashSet<long>();

            // 1. Resolve role-based targets
            if (request.RoleKeys?.Count > 0)
            {
                foreach (var roleKey in request.RoleKeys)
                {
                    try
                    {
                        var role = _roleService.SelectRoleByKeyUnfiltered(roleKey);
                        Console.WriteLine($"[NotificationDispatcher] Role '{roleKey}' resolved: {(role != null ? $"RoleId={role.RoleId}" : "NOT FOUND")}");
                        if (role != null)
                        {
                            var usersInRole = _userRoleService.GetSysUsersByRoleId(role.RoleId);
                            Console.WriteLine($"[NotificationDispatcher] Role '{roleKey}' has {usersInRole?.Count ?? 0} users");
                            foreach (var user in usersInRole)
                            {
                                targetUserIds.Add(user.UserId);
                            }
                        }
                        else
                        {
                            _logger.Warn("Role key '{RoleKey}' not found in sys_role table", roleKey);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[NotificationDispatcher] ERROR resolving role '{roleKey}': {ex.Message}");
                        _logger.Error(ex, "Failed to resolve users for role '{RoleKey}'", roleKey);
                    }
                }
            }

            // 2. Resolve employee-based targets
            if (request.EmployeeIds?.Count > 0)
            {
                foreach (var employeeId in request.EmployeeIds)
                {
                    try
                    {
                        var employee = await _employeeService.GetByIdAsync(employeeId);
                        if (employee?.UserId != null && employee.UserId.Value > 0)
                        {
                            targetUserIds.Add(employee.UserId.Value);
                        }
                        else
                        {
                            _logger.Warn("Employee {EmployeeId} has no linked user account, skipping notification", employeeId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Failed to resolve user for employee {EmployeeId}", employeeId);
                    }
                }
            }

            // 3. Add direct user IDs
            if (request.UserIds?.Count > 0)
            {
                foreach (var userId in request.UserIds)
                {
                    targetUserIds.Add(userId);
                }
            }

            Console.WriteLine($"[NotificationDispatcher] Total target users resolved: {targetUserIds.Count} => [{string.Join(",", targetUserIds)}]");

            if (targetUserIds.Count == 0)
            {
                Console.WriteLine("[NotificationDispatcher] WARNING: No target users resolved, skipping notification");
                _logger.Warn("No target users resolved for notification: {Title}", request.Payload?.Title);
                return;
            }

            // 4. Batch insert persistent notification records
            await InsertNotificationRecords(request.Payload, targetUserIds);

            // 5. Push real-time via SignalR
            await PushToOnlineUsers(request.Payload, targetUserIds);
        }

        public async Task SendToUsersAsync(NotificationPayload payload, List<long> userIds)
        {
            await SendAsync(new NotificationRequest { Payload = payload, UserIds = userIds });
        }

        public async Task SendToRoleAsync(NotificationPayload payload, string roleKey)
        {
            await SendAsync(new NotificationRequest { Payload = payload, RoleKeys = new List<string> { roleKey } });
        }

        public async Task SendToRolesAsync(NotificationPayload payload, params string[] roleKeys)
        {
            await SendAsync(new NotificationRequest { Payload = payload, RoleKeys = roleKeys.ToList() });
        }

        private async Task InsertNotificationRecords(NotificationPayload payload, HashSet<long> userIds)
        {
            try
            {
                // 1. Create ONE content row
                var notification = new ErpNotification
                {
                    Title = payload.Title,
                    TitleAr = payload.TitleAr,
                    Message = payload.Message,
                    MessageAr = payload.MessageAr,
                    NotificationType = payload.NotificationType ?? "system",
                    ReferenceType = payload.ReferenceType,
                    ReferenceId = payload.ReferenceId,
                    Severity = payload.Severity ?? "info",
                    Module = payload.Module,
                    ActionUrl = payload.ActionUrl,
                    SenderUserId = payload.SenderUserId,
                    SenderName = payload.SenderName,
                    TargetScope = "targeted",
                    IsDeleted = false
                };

                Console.WriteLine($"[NotificationDispatcher] Inserting notification content: Title={notification.Title}, Type={notification.NotificationType}");
                notification = _notificationService.CreateNotification(notification);
                Console.WriteLine($"[NotificationDispatcher] Notification created with ID={notification.NotificationId}");

                // 2. Create N recipient rows
                var recipients = userIds.Select(userId => new ErpNotificationRecipient
                {
                    RecipientId = SnowFlakeSingle.Instance.NextId(),
                    NotificationId = notification.NotificationId,
                    UserId = userId,
                    IsRead = false,
                    IsDeleted = false,
                    CreateTime = DateTime.Now
                }).ToList();

                var insertedCount = await _notificationService.BulkInsertRecipientsAsync(recipients);
                Console.WriteLine($"[NotificationDispatcher] Inserted {insertedCount} recipient rows");

                _logger.Info("Inserted 1 notification + {Count} recipients for {Type}:{Id}",
                    recipients.Count, payload.ReferenceType, payload.ReferenceId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationDispatcher] ERROR inserting notification records: {ex.Message}");
                Console.WriteLine($"[NotificationDispatcher] Stack: {ex.StackTrace}");
                _logger.Error(ex, "Failed to insert notification records");
            }
        }

        private async Task PushToOnlineUsers(NotificationPayload payload, HashSet<long> targetUserIds)
        {
            try
            {
                var onlineConnectionIds = MessageHub.OnlineClients.Values
                    .Where(c => targetUserIds.Contains(c.Userid))
                    .Select(c => c.ConnnectionId)
                    .ToList();

                if (onlineConnectionIds.Count > 0)
                {
                    await _hubContext.Clients.Clients(onlineConnectionIds)
                        .SendAsync(HubsConstant.ReceiveNotification, payload);

                    Console.WriteLine($"[NotificationDispatcher] Pushed real-time notification to {onlineConnectionIds.Count} online connections");
                }
                else
                {
                    Console.WriteLine("[NotificationDispatcher] No online users to push to");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationDispatcher] ERROR pushing SignalR: {ex.Message}");
                _logger.Error(ex, "Failed to push real-time notification via SignalR");
            }
        }
    }
}
