using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalR.SelfHost
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            string conn = ConfigurationManager.AppSettings["Microsoft.SignalR.ConnectionString"];
            string backPlane = ConfigurationManager.AppSettings["Microsoft.SignalR.BackPlane"];
            switch (backPlane)
            {
                case "Redis":
                    GlobalHost.DependencyResolver.UseRedis(new RedisScaleoutConfiguration(conn, "perf"));
                    break;
                case "SqlServer":
                    GlobalHost.DependencyResolver.UseSqlServer(conn);
                    break;
                case "MessageBus":
                    GlobalHost.DependencyResolver.UseServiceBus(conn, "jwperf");
                    break;
                default:
                    break;
            }

            app.UseErrorPage();

            app.Map("/PerfConnection", map =>
            {
                // Turns cors support on allowing everything
                // In real applications, the origins should be locked down
                map.UseCors(CorsOptions.AllowAll)
                   .RunSignalR<PerfConnection>();
            });

            app.Map("/signalr", map =>
            {
                var config = new HubConfiguration
                {
                    // You can enable JSONP by uncommenting this line
                    // JSONP requests are insecure but some older browsers (and some
                    // versions of IE) require JSONP to work cross domain
                    // EnableJSONP = true
                };

                // Turns cors support on allowing everything
                // In real applications, the origins should be locked down
                map.UseCors(CorsOptions.AllowAll)
                   .RunSignalR(config);
            });

            // Turn tracing on programmatically
            GlobalHost.TraceManager.Switch.Level = SourceLevels.Information;
        }
    }
}
