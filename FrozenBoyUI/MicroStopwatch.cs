using System;

namespace FrozenBoyUI {
    public class MicroStopwatch : System.Diagnostics.Stopwatch {
        readonly double _microSecPerTick = 1_000_000D / System.Diagnostics.Stopwatch.Frequency;

        public MicroStopwatch() {
            if (!System.Diagnostics.Stopwatch.IsHighResolution) {
                throw new Exception("High-resolution performance counter is not available");
            }
        }

        public long ElapsedMicroseconds {
            get {
                return (long)(ElapsedTicks * _microSecPerTick);
            }
        }
    }
}
