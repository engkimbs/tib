using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Runtime;

namespace RVLat
{

    public class HiResTimer
    {
        private bool perfCounterIsSupported = false;
        private Int64 frequency = 0;


        private const string lib = "Kernel32.dll";
        [DllImport(lib)]
        private static extern int QueryPerformanceCounter(ref Int64 count);
        [DllImport(lib)]
        private static extern int QueryPerformanceFrequency(ref Int64 frequency);

        public HiResTimer()
        {
            int returnVal = QueryPerformanceFrequency(ref frequency);

            if (returnVal != 0 && frequency != 1000)
            {
                perfCounterIsSupported = true;
            }
            else
            {
                frequency = 1000;
            }
        }

        public Int64 TimeIntervalInTicks(Int64 start, Int64 end)
        {
            return (end - start);
        }

        public double TimeIntervalInSeconds(Int64 start, Int64 end)
        {
            Int64 timeElapsedInTicks = TimeIntervalInTicks(start, end);

            double value = (timeElapsedInTicks * 1.0) / this.Frequency;

            return value;
        }

        public double TimeIntervalInMilliseconds(Int64 start, Int64 end)
        {
            Int64 timeElapsedInTicks = TimeIntervalInTicks(start, end);

            double value = (timeElapsedInTicks * 1000.0) / this.Frequency;

            return value;
        }

        public double TimeIntervalInMicroseconds(Int64 start, Int64 end)
        {
            Int64 timeElapsedInTicks = TimeIntervalInTicks(start, end);

            double value = (timeElapsedInTicks * 1000000.0) / this.Frequency;

            return value;
        }

        public Int64 Frequency
        {
            get
            {
                return frequency;
            }
        }

        public Int64 Value
        {
            get
            {
                Int64 tickCount = 0;

                if (perfCounterIsSupported)
                {
                    // Get the value here if the counter is supported.
                    QueryPerformanceCounter(ref tickCount);

                    return tickCount;
                }
                else
                {
                    // Otherwise, use Environment.TickCount.
                    return (Int64)System.Environment.TickCount;
                }
            }
        }
    }

}
