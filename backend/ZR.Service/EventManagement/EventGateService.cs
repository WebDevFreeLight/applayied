using Infrastructure.Attribute;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Repository;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Service.EventManagement
{
    /// <summary>
    /// Event Gate - بوابات الدخولService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IEventGateService), ServiceLifetime = LifeTime.Transient)]
    public class EventGateService : BaseService<EventGate>, IEventGateService
    {
        private readonly IEventGateHistoryService _historyService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EventGateService(
            IEventGateHistoryService historyService,
            IHttpContextAccessor httpContextAccessor)
        {
            _historyService = historyService;
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// 查询Event Gate - بوابات الدخول列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<EventGateDto> GetList(EventGateQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Gateid asc")
                .Where(predicate.ToExpression())
                .ToPage<EventGate, EventGateDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Gateid"></param>
        /// <returns></returns>
        public EventGate GetInfo(long Gateid)
        {
            var response = Queryable()
                .Where(x => x.Gateid == Gateid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加Event Gate - بوابات الدخول
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public EventGate AddEventGate(EventGate model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();

            // Log history
            var history = new EventGateHistory
            {
                GateId = model.Gateid,
                GateCode = model.Gatecode,
                GateName = model.Gatename,
                EventId = model.Eventid,
                ActionType = "created",
                NewStatus = model.Status,
                NewGateType = model.Gatetype,
                NewCapacity = model.Maxcapacityperhour,
                NewManagerId = model.Gatemanagerid,
                PerformedByUserId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value != null
                    ? long.Parse(_httpContextAccessor.HttpContext.User.FindFirst("UserId").Value)
                    : (long?)null,
                PerformedByUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name,
                IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                Notes = $"Gate {model.Gatecode} created"
            };
            _historyService.LogHistory(history);

            return model;
        }

        /// <summary>
        /// 修改Event Gate - بوابات الدخول
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateEventGate(EventGate model)
        {
            // Get current gate info before update
            var currentGate = GetInfo(model.Gateid);

            var result = Update(model, true, "修改Event Gate - بوابات الدخول");

            // Log history if update was successful
            if (result > 0)
            {
                var changes = new List<string>();

                // Track status changes
                if (currentGate.Status != model.Status)
                {
                    changes.Add($"Status changed from {currentGate.Status} to {model.Status}");
                }

                // Track gate type changes
                if (currentGate.Gatetype != model.Gatetype)
                {
                    changes.Add($"Gate type changed from {currentGate.Gatetype} to {model.Gatetype}");
                }

                // Track capacity changes
                if (currentGate.Maxcapacityperhour != model.Maxcapacityperhour)
                {
                    changes.Add($"Capacity changed from {currentGate.Maxcapacityperhour} to {model.Maxcapacityperhour}");
                }

                // Track manager changes
                if (currentGate.Gatemanagerid != model.Gatemanagerid)
                {
                    changes.Add($"Manager changed");
                }

                var history = new EventGateHistory
                {
                    GateId = model.Gateid,
                    GateCode = model.Gatecode,
                    GateName = model.Gatename,
                    EventId = model.Eventid,
                    ActionType = "updated",
                    PreviousStatus = currentGate.Status,
                    NewStatus = model.Status,
                    PreviousGateType = currentGate.Gatetype,
                    NewGateType = model.Gatetype,
                    PreviousCapacity = currentGate.Maxcapacityperhour,
                    NewCapacity = model.Maxcapacityperhour,
                    PreviousManagerId = currentGate.Gatemanagerid,
                    NewManagerId = model.Gatemanagerid,
                    PerformedByUserId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value != null
                        ? long.Parse(_httpContextAccessor.HttpContext.User.FindFirst("UserId").Value)
                        : (long?)null,
                    PerformedByUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name,
                    IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    Notes = changes.Count > 0 ? string.Join(", ", changes) : "Gate information updated"
                };
                _historyService.LogHistory(history);
            }

            return result;
        }

        /// <summary>
        /// 清空Event Gate - بوابات الدخول
        /// </summary>
        /// <returns></returns>
        public bool TruncateEventGate()
        {
            var newTableName = $"event_gates_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("event_gates", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入Event Gate - بوابات الدخول
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportEventGate(List<EventGate> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Eventid.IsEmpty(), "Event ID - معرف الفعالية不能为空")
                .SplitError(x => x.Item.Zoneid.IsEmpty(), "Zone ID - معرف المنطقة (اختياري)不能为空")
                .SplitError(x => x.Item.Gatecode.IsEmpty(), "Gate Code - رمز البوابة不能为空")
                .SplitError(x => x.Item.Gatename.IsEmpty(), "Gate Name (English) - اسم البوابة بالإنجليزية不能为空")
                //.WhereColumns(it => it.UserName)//如果不是主键可以这样实现（多字段it=>new{it.x1,it.x2}）
                .ToStorage();
            var result = x.AsInsertable.ExecuteCommand();//插入可插入部分;

            string msg = $"插入{x.InsertList.Count} 更新{x.UpdateList.Count} 错误数据{x.ErrorList.Count} 不计算数据{x.IgnoreList.Count} 删除数据{x.DeleteList.Count} 总共{x.TotalList.Count}";                    
            Console.WriteLine(msg);

            //输出错误信息               
            foreach (var item in x.ErrorList)
            {
                Console.WriteLine("错误" + item.StorageMessage);
            }
            foreach (var item in x.IgnoreList)
            {
                Console.WriteLine("忽略" + item.StorageMessage);
            }

            return (msg, x.ErrorList, x.IgnoreList);
        }

        /// <summary>
        /// 导出Event Gate - بوابات الدخول
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<EventGateDto> ExportList(EventGateQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new EventGateDto()
                {
                    EventidLabel = it.Eventid.GetConfigValue<Model.System.SysDictData>("sql_event"),
                    ZoneidLabel = it.Zoneid.GetConfigValue<Model.System.SysDictData>("sql_blv_boulevard_season_zones"),
                    GatetypeLabel = it.Gatetype.GetConfigValue<Model.System.SysDictData>("sql_gate_type"),
                    LocationLabel = it.Location.GetConfigValue<Model.System.SysDictData>("sql_location"),
                    StatusLabel = it.Status.GetConfigValue<Model.System.SysDictData>("sql_status"),
                    HasscannerLabel = it.Hasscanner.GetConfigValue<Model.System.SysDictData>("sys_common_status"),
                    GatemanageridLabel = it.Gatemanagerid.GetConfigValue<Model.System.SysDictData>("sql_hr_employee"),
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<EventGate> QueryExp(EventGateQueryDto parm)
        {
            var predicate = Expressionable.Create<EventGate>();

            predicate = predicate.AndIF(parm.Eventid != null, it => it.Eventid == parm.Eventid);
            predicate = predicate.AndIF(parm.Zoneid != null, it => it.Zoneid == parm.Zoneid);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Gatecode), it => it.Gatecode == parm.Gatecode);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Gatename), it => it.Gatename.Contains(parm.Gatename));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Gatenamear), it => it.Gatenamear.Contains(parm.Gatenamear));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Description), it => it.Description.Contains(parm.Description));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Gatetype), it => it.Gatetype == parm.Gatetype);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Location), it => it.Location.Contains(parm.Location));
            predicate = predicate.AndIF(parm.Maxcapacityperhour != null, it => it.Maxcapacityperhour >= parm.Maxcapacityperhour);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);


            predicate = predicate.AndIF(parm.BeginOpeningtime != null, it => it.Openingtime >= parm.BeginOpeningtime);
            predicate = predicate.AndIF(parm.EndOpeningtime != null, it => it.Openingtime <= parm.EndOpeningtime);



            predicate = predicate.AndIF(parm.BeginClosingtime != null, it => it.Closingtime >= parm.BeginClosingtime);
            predicate = predicate.AndIF(parm.EndClosingtime != null, it => it.Closingtime <= parm.EndClosingtime);

            //predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Closingtime), it => it.Closingtime == parm.Closingtime);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Hasscanner), it => it.Hasscanner == parm.Hasscanner);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Scannerdeviceid), it => it.Scannerdeviceid == parm.Scannerdeviceid);
            predicate = predicate.AndIF(parm.Gatemanagerid != null, it => it.Gatemanagerid == parm.Gatemanagerid);
            predicate = predicate.AndIF(parm.Displayorder != null, it => it.Displayorder >= parm.Displayorder);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Isactive), it => it.Isactive == parm.Isactive);
          //  predicate = predicate.AndIF(parm.BeginLaststatusupdate == null, it => it.Laststatusupdate >= DateTime.Now.ToShortDateString().ParseToDateTime());
            predicate = predicate.AndIF(parm.BeginLaststatusupdate != null, it => it.Laststatusupdate >= parm.BeginLaststatusupdate);
            predicate = predicate.AndIF(parm.EndLaststatusupdate != null, it => it.Laststatusupdate <= parm.EndLaststatusupdate);
          //  predicate = predicate.AndIF(parm.BeginCreateTime == null, it => it.CreateTime >= DateTime.Now.ToShortDateString().ParseToDateTime());
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
          //  predicate = predicate.AndIF(parm.BeginUpdateTime == null, it => it.UpdateTime >= DateTime.Now.ToShortDateString().ParseToDateTime());
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time <= parm.EndUpdateTime);
            return predicate;
        }

        /// <summary>
        /// 获取指定活动的所有门禁
        /// Get all gates for a specific event
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <returns>List of gates</returns>
        public List<EventGate> GetGatesByEventId(long eventId)
        {
            var gates = Queryable()
                .Where(x => x.Eventid == eventId && x.Isactive == "Y")
                .OrderBy(x => x.Displayorder)
                .ToList();

            return gates;
        }

        /// <summary>
        /// 更新门禁状态
        /// Update gate status
        /// </summary>
        /// <param name="gateId">Gate ID</param>
        /// <param name="status">New status (open, closed, maintenance, full)</param>
        /// <returns>Number of rows affected</returns>
        public int UpdateGateStatus(long gateId, string status)
        {
            var gate = Queryable().Where(x => x.Gateid == gateId).First();

            if (gate == null)
            {
                return 0;
            }

            var previousStatus = gate.Status;
            gate.Status = status;
            gate.Laststatusupdate = DateTime.Now;

            var result = Update(gate, true, "更新门禁状态 Update Gate Status");

            // Log history with specific action types
            if (result > 0)
            {
                string actionType = "status_changed";
                string notes = $"Gate status changed from {previousStatus} to {status}";

                // Determine specific action type based on new status
                switch (status.ToLower())
                {
                    case "open":
                    case "opened":
                        actionType = "opened";
                        notes = $"Gate opened (was {previousStatus})";
                        break;
                    case "closed":
                        actionType = "closed";
                        notes = $"Gate closed (was {previousStatus})";
                        break;
                    case "maintenance":
                        actionType = "maintenance";
                        notes = $"Gate set to maintenance mode (was {previousStatus})";
                        break;
                    case "full":
                        notes = $"Gate marked as full (was {previousStatus})";
                        break;
                }

                var history = new EventGateHistory
                {
                    GateId = gateId,
                    GateCode = gate.Gatecode,
                    GateName = gate.Gatename,
                    EventId = gate.Eventid,
                    ActionType = actionType,
                    PreviousStatus = previousStatus,
                    NewStatus = status,
                    NewGateType = gate.Gatetype,
                    NewCapacity = gate.Maxcapacityperhour,
                    NewManagerId = gate.Gatemanagerid,
                    PerformedByUserId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value != null
                        ? long.Parse(_httpContextAccessor.HttpContext.User.FindFirst("UserId").Value)
                        : (long?)null,
                    PerformedByUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name,
                    IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    Notes = notes
                };
                _historyService.LogHistory(history);
            }

            return result;
        }

        /// <summary>
        /// 更新门禁队列计数
        /// Update gate queue count
        /// </summary>
        /// <param name="gateId">Gate ID</param>
        /// <param name="queueCount">Current queue count</param>
        /// <returns>Number of rows affected</returns>
        public int UpdateQueueCount(long gateId, int queueCount)
        {
            // Note: This method assumes there is a Currentqueuecount field in the EventGate model
            // If the field doesn't exist, you need to add it to the EventGate model first:
            // public int Currentqueuecount { get; set; }

            var result = Context.Updateable<EventGate>()
                .SetColumns(it => new EventGate
                {
                    // Currentqueuecount = queueCount,  // Uncomment when field is added to model
                    Update_time = DateTime.Now
                })
                .Where(it => it.Gateid == gateId)
                .ExecuteCommand();

            return result;
        }
    }
}
