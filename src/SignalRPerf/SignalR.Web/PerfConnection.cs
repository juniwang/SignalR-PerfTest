﻿using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SignalR.Web
{
    public class PerfConnection : PersistentConnection
    {
        internal static ConnectionBehavior Behavior { get; set; }

        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            if (Behavior == ConnectionBehavior.Echo)
            {
                Connection.Send(connectionId, data);
            }
            else if (Behavior == ConnectionBehavior.Broadcast)
            {
                Connection.Broadcast(data);
            }
            return Task.FromResult<object>(null);
        }
    }

    public enum ConnectionBehavior
    {
        ListenOnly,
        Echo,
        Broadcast
    }
}