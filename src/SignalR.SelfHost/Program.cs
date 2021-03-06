﻿using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalR.SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://+:8080"))
            {
                DashboardHub.Init();

                Console.WriteLine("Server running at http://+:8080/");
                Console.ReadLine();
            }
        }
    }
}
