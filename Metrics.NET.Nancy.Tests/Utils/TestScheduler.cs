using System;
using System.Threading;
using System.Threading.Tasks;
using Metrics.Utils;

namespace Metrics.NET.Nancy.Tests.Utils
{
    /// <summary>
    /// Utility class for manually executing the scheduled task.
    /// </summary>
    /// <remarks>
    /// This class is useful for testing.
    /// </remarks>
    public sealed class TestScheduler : Scheduler
    {
        private readonly TestClock _clock;
        private TimeSpan _interval;
        private Action<CancellationToken> _action;
        private long _lastRun = 0;

        public TestScheduler(TestClock clock)
        {
            _clock = clock;
            _clock.Advanced += (s, l) => RunIfNeeded();
        }

        public void Start(TimeSpan interval, Func<CancellationToken, Task> task)
        {
            Start(interval, (t) => task(t).Wait());
        }

        public void Start(TimeSpan interval, Func<Task> task)
        {
            Start(interval, () => task().Wait());
        }

        public void Start(TimeSpan interval, Action action)
        {
            Start(interval, t => action());
        }

        public void Start(TimeSpan interval, Action<CancellationToken> action)
        {
            if (interval.TotalSeconds == 0)
            {
                throw new ArgumentException("interval must be > 0 seconds", "interval");
            }

            _interval = interval;
            _lastRun = _clock.Seconds;
            _action = action;
        }

        private void RunIfNeeded()
        {
            long clockSeconds = _clock.Seconds;
            long elapsed = clockSeconds - _lastRun;
            var times = elapsed / _interval.TotalSeconds;
            using (CancellationTokenSource ts = new CancellationTokenSource())
                while (times-- >= 1)
                {
                    _lastRun = clockSeconds;
                    _action(ts.Token);
                }
        }

        public void Stop() { }
        public void Dispose() { }
    }
}