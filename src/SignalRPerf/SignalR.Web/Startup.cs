using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using SignalR.Web;
using System;

[assembly: OwinStartup(typeof(Startup))]
namespace SignalR.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(60);

            app.MapSignalR<PerfConnection>("/PerfConnection");
            app.MapSignalR();

            DashboardHub.Init();
        }

    }
}
