
namespace Metrics.NET.Nancy.Sample.Common
{
    public class MultiContextMetrics
    {
        private readonly Counter _firstCounter = Metric.Context("First Context").Counter("Counter", Unit.Requests);

        private readonly Counter _secondCounter = Metric.Context("Second Context").Counter("Counter", Unit.Requests);
        private readonly Meter _secondMeter = Metric.Context("Second Context").Meter("Meter", Unit.Errors, TimeUnit.Seconds);

        public void Run()
        {
            _firstCounter.Increment();
            _secondCounter.Increment();
            _secondMeter.Mark();
        }
    }

    public class MultiContextInstanceMetrics
    {
        private readonly Counter _instanceCounter;
        private readonly Timer _instanceTimer;

        public MultiContextInstanceMetrics(string instanceName)
        {
            var context = Metric.Context(instanceName);

            _instanceCounter = context.Counter("Sample Counter", Unit.Errors);
            _instanceTimer = context.Timer("Sample Timer", Unit.Requests);
        }

        public void Run()
        {
            using (var context = _instanceTimer.NewContext())
            {
                _instanceCounter.Increment();
            }
        }

        public static void RunSample()
        {
            for (int i = 0; i < 5; i++)
            {
                new MultiContextInstanceMetrics("Sample Instance " + i.ToString()).Run();
            }
        }
    }
}
