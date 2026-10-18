using System;
using System.Linq;
using Infrastructure.Attribute;
using Microsoft.AspNetCore.SignalR;
using ZR.Model.System.Enums;
using ZR.ServiceCore.Services;

namespace ZR.ServiceCore.Signalr
{
    /// <summary>
    /// Delivers an in-app notification two ways at once:
    ///   1) persists it as a SysUserMsg row so it appears in the user's Messages / header bell, and
    ///   2) pushes it in real time over SignalR (<c>receiveNotification</c>) to every live connection
    ///      of that user, so a toast/popup can appear instantly without a refresh.
    /// Used by the PMO automation rules and by the Notices feature. Never throws: a delivery failure
    /// must not break the business operation that raised it.
    /// </summary>
    public interface IPmoNotifier
    {
        /// <summary>Notify one user (persist + realtime). Returns the stored message id, or 0 on failure.</summary>
        long Notify(long? userId, string title, string body, string type = "SYSTEM");

        /// <summary>Broadcast a notice to all connected clients (realtime only), e.g. a published notice.</summary>
        void Broadcast(string title, string body, string type = "NOTICE");
    }

    [AppService(ServiceType = typeof(IPmoNotifier), ServiceLifetime = LifeTime.Transient)]
    public class PmoNotifier : IPmoNotifier
    {
        private readonly ISysUserMsgService _userMsg;
        private readonly IHubContext<MessageHub> _hub;

        public PmoNotifier(ISysUserMsgService userMsg, IHubContext<MessageHub> hub)
        {
            _userMsg = userMsg;
            _hub = hub;
        }

        public long Notify(long? userId, string title, string body, string type = "SYSTEM")
        {
            long msgId = 0;
            try
            {
                var content = string.IsNullOrWhiteSpace(body) ? title : $"{title} — {body}";
                if (userId is > 0)
                {
                    var saved = _userMsg.AddSysUserMsg(userId.Value, content, UserMsgType.SYSTEM);
                    msgId = saved?.MsgId ?? 0;
                }

                var payload = new { msgId, title, body, type, time = DateTime.Now };
                var connections = MessageHub.OnlineClients.Values
                    .Where(u => userId == null || u.Userid == userId)
                    .Select(u => u.ConnnectionId).ToList();

                if (connections.Count > 0)
                    _hub.Clients.Clients(connections).SendAsync(HubsConstant.ReceiveNotification, payload);
            }
            catch { /* delivery is best-effort; never break the caller */ }
            return msgId;
        }

        public void Broadcast(string title, string body, string type = "NOTICE")
        {
            try
            {
                var payload = new { title, body, type, time = DateTime.Now };
                _hub.Clients.All.SendAsync(HubsConstant.ReceiveNotification, payload);
            }
            catch { /* best-effort */ }
        }
    }
}
