using System;
using System.Collections.Generic;
using System.Threading;
using Metrics.Core;
using Metrics.MetricData;

namespace Metrics.NET.Nancy.Sample.Common
{
    public class SampleMetrics
    {
        /// <summary>
        /// keep the total count of the requests
        /// </summary>
        private readonly Counter _totalRequestsCounter = Metric.Counter("Requests", Unit.Requests);

        /// <summary>
        /// count the current concurrent requests
        /// </summary>
        private readonly Counter _concurrentRequestsCounter = Metric.Counter("SampleMetrics.ConcurrentRequests", Unit.Requests);

        private readonly Counter _setCounter = Metric.Counter("Set Counter", Unit.Items);

        private readonly Meter _setMeter = Metric.Meter("Set Meter", Unit.Items);

        /// <summary>
        /// keep a histogram of the input data of our request method 
        /// </summary>
        private readonly Histogram _histogramOfData = Metric.Histogram("ResultsExample", Unit.Items);

        /// <summary>
        /// measure the rate at which requests come in
        /// </summary>
        private readonly Meter _meter = Metric.Meter("Requests", Unit.Requests);

        /// <summary>
        /// measure the time rate and duration of requests
        /// </summary>
        private readonly Timer _timer = Metric.Timer("Requests", Unit.Requests);

        private double _someValue = 1;

        public SampleMetrics()
        {
            // define a simple gauge that will provide the instant value of this.someValue when requested
            Metric.Gauge("SampleMetrics.DataValue", () => _someValue, Unit.Custom("$"));

            Metric.Gauge("Custom Ratio", () => ValueReader.GetCurrentValue(_totalRequestsCounter).Count / ValueReader.GetCurrentValue(_meter).FiveMinuteRate, Unit.Percent);

            Metric.Advanced.Gauge("Ratio", () => new HitRatioGauge(_meter, _timer, m => m.OneMinuteRate), Unit.Percent);
        }

        public void Request(int i)
        {
            new MultiContextMetrics().Run();
            MultiContextInstanceMetrics.RunSample();

            using (_timer.NewContext(i.ToString())) // measure until disposed
            {
                _someValue *= (i + 1); // will be reflected in the gauge 

                _concurrentRequestsCounter.Increment(); // increment concurrent requests counter

                _totalRequestsCounter.Increment(); // increment total requests counter 

                _meter.Mark(); // signal a new request to the meter

                _histogramOfData.Update(new Random().Next(5000), i.ToString()); // update the histogram with the input data

                var item = "Item " + new Random().Next(5);
                _setCounter.Increment(item);

                _setMeter.Mark(item);

                // simulate doing some work
                int ms = Math.Abs((int)(new Random().Next(3000)));
                Thread.Sleep(ms);

                _concurrentRequestsCounter.Decrement(); // decrement number of concurrent requests
            }
        }


        public static void RunSomeRequests()
        {
            SampleMetrics test = new SampleMetrics();
            List<Thread> tasks = new List<Thread>();
            for (int i = 0; i < 10; i++)
            {
                int j = i;
                tasks.Add(new Thread(() => test.Request(j)));
            }

            tasks.ForEach(t => t.Start());
            tasks.ForEach(t => t.Join());
        }
    }
}
