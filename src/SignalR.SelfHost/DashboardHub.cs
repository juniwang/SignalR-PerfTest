using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalR.SelfHost
{
    public class DashboardHub : Hub
    {
        private static int _broadcastSize = 32;
        private static string _broadcastPayload;
        private static int _broadcastCount = 1;
        private static int _broadcastSeconds = 1;
        private static bool _batchingEnabled;
        private static int _actualFps = 0;

        private static readonly Lazy<HighFrequencyTimer> _timerInstance = new Lazy<HighFrequencyTimer>(() =>
        {
            var clients = GlobalHost.ConnectionManager.GetHubContext<DashboardHub>().Clients;
            var connection = GlobalHost.ConnectionManager.GetConnectionContext<PerfConnection>().Connection;
            return new HighFrequencyTimer(1,
                _ =>
                {
                    if (_batchingEnabled)
                    {
                        var count = _broadcastCount;
                        for (var i = 0; i < count; i++)
                        {
                            var payloadWithTimestamp = "C" + DateTime.UtcNow.Ticks.ToString() + "|" + _broadcastPayload;
                            connection.Broadcast(payloadWithTimestamp);
                        }
                    }
                    else
                    {
                        var payloadWithTimestamp = "C" + DateTime.UtcNow.Ticks.ToString() + "|" + _broadcastPayload;
                        connection.Broadcast(payloadWithTimestamp);
                    }
                },
                () => clients.All.started(),
                () => { clients.All.stopped(); clients.All.serverFps(0); },
                fps => { _actualFps = fps; clients.All.serverFps(fps); }
            );
        });

        private HighFrequencyTimer _timer { get { return _timerInstance.Value; } }

        internal static void Init()
        {
            SetBroadcastPayload();
        }

        public dynamic GetStatus()
        {
            return new
            {
                ConnectionBehavior = PerfConnection.Behavior,
                BroadcastBatching = _batchingEnabled,
                BroadcastCount = _broadcastCount,
                BroadcastSeconds = _broadcastSeconds,
                BroadcastSize = _broadcastSize,
                Broadcasting = _timer.IsRunning(),
                ServerFps = _actualFps
            };
        }

        public void SetConnectionBehavior(ConnectionBehavior behavior)
        {
            PerfConnection.Behavior = behavior;
            Console.WriteLine("Connection behavior set to: " + behavior.ToString());
            Clients.Others.connectionBehaviorChanged(((int)behavior).ToString());
        }

        public void SetBroadcastBehavior(bool batchingEnabled)
        {
            _batchingEnabled = batchingEnabled;
            Console.WriteLine("Batch enabled");
            Clients.Others.broadcastBehaviorChanged(batchingEnabled);
        }

        public void SetBroadcastRate(int count, int seconds)
        {
            // Need to turn the count/seconds into FPS
            _broadcastCount = count;
            _broadcastSeconds = seconds;
            _timer.FPS = _batchingEnabled ? 1 / (double)seconds : count;
            Clients.Others.broadcastRateChanged(count, seconds);
        }

        public void SetBroadcastSize(int size)
        {
            _broadcastSize = size;
            SetBroadcastPayload();
            Clients.Others.broadcastSizeChanged(size.ToString());
        }

        public void ForceGC()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine("Force GC done.");
        }

        public void StartBroadcast()
        {
            Console.WriteLine("Starting Broadcast...");
            _timer.Start();
        }

        public void StopBroadcast()
        {
            _timer.Stop();
            Console.WriteLine("Stop Broadcast...");
        }

        private static void SetBroadcastPayload()
        {
            _broadcastPayload = String.Join("", Enumerable.Range(0, _broadcastSize - 1).Select(i => "a"));
        }
    }
}
