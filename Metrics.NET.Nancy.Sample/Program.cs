﻿using System;
using System.Diagnostics;
using Metrics.NET.Nancy.Sample.Common;
using Metrics.Utils;
using Nancy.Hosting.Self;
using Newtonsoft.Json;

namespace Metrics.NET.Nancy.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings { Formatting = Formatting.Indented };

            using (ActionScheduler scheduler = new ActionScheduler())
            using (var host = new NancyHost(new Uri("http://localhost:8090")))
            {
                host.Start();
                Console.WriteLine("Nancy Running at http://localhost:8090");
                Console.WriteLine("Press any key to exit");

                Process.Start("http://localhost:8090/metrics/");

                SampleMetrics.RunSomeRequests();

                scheduler.Start(TimeSpan.FromMilliseconds(500), () =>
                {
                    SetCounterSample.RunSomeRequests();
                    SetMeterSample.RunSomeRequests();
                    UserValueHistogramSample.RunSomeRequests();
                    UserValueTimerSample.RunSomeRequests();
                    SampleMetrics.RunSomeRequests();
                });

                HealthChecksSample.RegisterHealthChecks();

                Console.ReadKey();
            }
        }
    }
}
