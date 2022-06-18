using System;
using System.Threading;
using TIBCO.Rendezvous;

namespace RVLat
{

    enum Mode
    {
        CLIENT,
        SERVER
    };

    class RVLat
    {

        public const string DEFAULT_RELIABILITY = null;
        public const string DEFAULT_DEBUG = null;
        public const int DEFAULT_MESSAGE_COUNT = 10000;
        public const int DEFAULT_BATCH_INTERVAL = 1;
        public const int DEFAULT_BATCH_SIZE = 1;
        public const string DEFAULT_INITIALIZATION_SUBJECT = "RVLAT.I";
        public const string DEFAULT_USE_INBOXES_FIELD_NAME = "RVLAT.UIB";
        public const double DEFAULT_SEARCH_TIMEOUT = 300;
        public const string DEFAULT_REQUEST_SUBJECT = "RVLAT.R";
        public const string DEFAULT_REPLY_SUBJECT = "RVLAT.RP";
        public const string DEFAULT_LOG_FILENAME = "log.txt";

        private static Mode mode;

        public static void RunPreliminaryDiagnostics()
        {
            long start, end;

            Console.WriteLine("******************** BEGINNING OF PRELIMINARY DIAGNOSTICS ********************");

            Console.WriteLine("******************** Core/CPU Inventory:");

            Console.WriteLine("The number of cores/processors on this host is {0}.", System.Environment.ProcessorCount);
            Console.WriteLine("Original ProcessorAffinity: {0}", System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity);

            Thread.Sleep(1000);

            Console.WriteLine("******************** MyStopwatch Status:");

            MyStopwatch.PrintStatus();

            Thread.Sleep(1000);

            Console.WriteLine("******************** Timing Thread.Sleep(1) using MyStopwatch:");

            for (int i = 0; i < 10; i++)
            {
                start = MyStopwatch.GetTimestamp();
                Thread.Sleep(1);
                end = MyStopwatch.GetTimestamp();

                Console.WriteLine("Thread.Sleep(1) took " + MyStopwatch.TimeSpanInMilliseconds(start, end) + " ms.");
            }

            Thread.Sleep(1000);

            Console.WriteLine("******************** Timing Thread.Sleep(30) using MyStopwatch:");

            for (int i = 0; i < 10; i++)
            {
                start = MyStopwatch.GetTimestamp();
                Thread.Sleep(30);
                end = MyStopwatch.GetTimestamp();

                Console.WriteLine("Thread.Sleep(30) took " + MyStopwatch.TimeSpanInMilliseconds(start, end) + " ms.");
            }

            Thread.Sleep(1000);

            Console.WriteLine("******************** Timing message updates using MyStopwatch:");

            Message message = new Message();
            message.UpdateField("sqn", Int32.MaxValue);
            message.UpdateField("t0", Int64.MaxValue);

            for (int i = 0; i < 10; i++)
            {
                start = MyStopwatch.GetTimestamp();
                message.UpdateField("sqn", Int32.MaxValue);
                message.UpdateField("t0", Int64.MaxValue);
                end = MyStopwatch.GetTimestamp();

                Console.WriteLine("Message updates took " + MyStopwatch.TimeSpanInMilliseconds(start, end) + " ms.");
            }

            Console.WriteLine("******************** END OF PRELIMINARY DIAGNOSTICS ********************");
        }

        public static void ParseParameters(string[] args)
        {
            if (args.Length < 1)
            {
                RVLat.Usage();
            }
            if ("srv".Equals(args[0]))
            {
                RVLat.mode = Mode.SERVER;
            }
            else if ("cli".Equals(args[0]))
            {
                RVLat.mode = Mode.CLIENT;
            }
            else
            {
                RVLat.Usage();
            }
        }

        public static void Usage()
        {
            Console.WriteLine("\nSUMMARY:\n");
            Console.WriteLine("RVLat srv|cli OPTIONS\n\n");

            Console.WriteLine("DESCRIPTION:\n");
            Console.WriteLine("The client (cli) sends requests to the server (srv). The server responds immediately.\n");
            Console.WriteLine("Latency is the round-trip time of the request-reply exchange. Start the server first.\n\n");

            Console.WriteLine("OPTIONS: \n");
            Console.WriteLine("[-network <string>]\n");
            Console.WriteLine("[-service <int>]\n");
            Console.WriteLine("[-daemon <int>]\n");
            Console.WriteLine("[-reliability <int>]    The reliability in seconds. (for IPM)\n");

            Console.WriteLine("\nclient-only (cli) options:\n");
            Console.WriteLine("[-messages <int>]       Run duration based on message count.\n");
            Console.WriteLine("[-time <double>]        Run duration based on time in seconds (holds precedence).\n");
            Console.WriteLine("[-batch <int>]          Batch size.\n");
            Console.WriteLine("[-interval <double>]    Batch interval in seconds.\n");
            Console.WriteLine("[-yield]                Yield after each send.\n");
            Console.WriteLine("[-dispose]              Dispose of messages as soon as possible.\n");
            Console.WriteLine("[-terse]                Limited comma-separated output (recommended for spreadsheets).\n");
            Console.WriteLine("[-datapoints]           Print {rtt} data-points.\n");
            Console.WriteLine("[-spikes <double>]      Print {sqn, rtt} greater-than <spikes> milliseconds.\n");
            Console.WriteLine("[-w <filename>]         Replace stdout with file.\n");
            Console.WriteLine("[-inbox]                Use INBOX (ptp).\n");

            System.Environment.Exit(0);
        }

        static void Main(string[] args)
        {

            RVLat.RunPreliminaryDiagnostics();

            RVLat.ParseParameters(args);

            if (RVLat.mode == Mode.CLIENT)
            {
                Client.execute(args);
            }
            else if (RVLat.mode == Mode.SERVER)
            {
                Server.execute(args);
            }
            else
            {
                RVLat.Usage();
            }
        }

    }

}
