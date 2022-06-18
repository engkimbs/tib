using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace RVLat
{

    // System.Diagnostics.Stopwatch is thread-safe, but it may yield incorrect results
    // if called from 2 threads running on different cores/CPUs.
    // Therefore, we must force all timestamp reading to happen in a single thread.
    // This necessary evil adds a significant overhead to the test.
    public sealed class MyStopwatch
    {

        static private Thread MasterThread = null;

        static private EventWaitHandle TickCountUpdateRequest = new EventWaitHandle(false, EventResetMode.AutoReset);
        static private EventWaitHandle TickCountUpdateComplete = new EventWaitHandle(false, EventResetMode.AutoReset);

        static private long CurrentTickCount = 0;


        private static void MasterThreadLoop()
        {

            // To make sure this thread keeps being scheduled on the same core/CPU.
            Thread.BeginThreadAffinity();

            while (true)
            {
                TickCountUpdateRequest.WaitOne();

                CurrentTickCount = System.Diagnostics.Stopwatch.GetTimestamp();

                TickCountUpdateComplete.Set();
            }
        }

        static MyStopwatch()
        {

            MyStopwatch.MasterThread = new Thread(new ThreadStart(MasterThreadLoop));

            MasterThread.IsBackground = true;
            // To make sure this thread this thread is scheduled immediately after a running thread blocks on GetTimestamp().
            // As such, any measurement will include the time needed to perform 2 context switches.
            // This price must be paid or no accurate measurements can be made on multi-core/CPU machines.
            MasterThread.Priority = ThreadPriority.Highest;

            MasterThread.Start();
        }

        public static void PrintStatus()
        {

            Console.WriteLine("Stopwatch.IsHighResolution = " + System.Diagnostics.Stopwatch.IsHighResolution);
            Console.WriteLine("Stopwatch.Frequency = " + System.Diagnostics.Stopwatch.Frequency + " (ticks per second)");

            // Sanity check

            long start = System.Diagnostics.Stopwatch.GetTimestamp();
            long end = System.Diagnostics.Stopwatch.GetTimestamp();

            Console.WriteLine("Elapsed Ticks = " + (end - start));
            Console.WriteLine("Resolution = " + (((end - start) / (double)System.Diagnostics.Stopwatch.Frequency) * 1000000d) + " us");

            Console.Out.Flush();
        }

        public static long GetTimestamp()
        {

            lock (MasterThread)
            {
                MyStopwatch.TickCountUpdateRequest.Set();
                MyStopwatch.TickCountUpdateComplete.WaitOne();

                return CurrentTickCount;
            }
        }

        public static long TimeSpanInTicks(long start, long end)
        {

            return (end - start);
        }

        public static double TimeSpanInSeconds(long start, long end)
        {

            return (end - start) / ((double)System.Diagnostics.Stopwatch.Frequency);
        }

        public static double TimeSpanInMilliseconds(long start, long end)
        {

            return ((end - start) * 1000d) / ((double)System.Diagnostics.Stopwatch.Frequency);
        }

        public static double TimeSpanInMicroseconds(long start, long end)
        {

            return ((end - start) * 1000000d) / ((double)System.Diagnostics.Stopwatch.Frequency);
        }

    }

}

