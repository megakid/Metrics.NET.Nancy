using System;
using Metrics.Utils;

namespace Metrics.NET.Nancy.Tests.Utils
{
    public sealed class TestClock : Clock
    {
        private long _nanoseconds = 0;

        public override long Nanoseconds => _nanoseconds;

        public override DateTime UTCDateTime => new DateTime(_nanoseconds / 100L, DateTimeKind.Utc);

        public void Advance(TimeUnit unit, long value)
        {
            _nanoseconds += unit.ToNanoseconds(value);
            Advanced?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Advanced;
    }
}