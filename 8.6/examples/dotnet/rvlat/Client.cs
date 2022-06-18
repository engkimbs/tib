using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;
using TIBCO.Rendezvous;
using System.Runtime;

namespace RVLat
{

    class Client
    {

        private string service;
        private string network;
        private string daemon;

        private string reliability;
        private string debug;

        private bool useInboxes;

        private int messageCount;
        private bool packAndUnpack;
        private int batchSize;
        private double batchInterval;
        private bool yieldAfterEachSend;
        private bool useDispose;

        private double outlierThreshold;
        private double timeLimit;

        private string logFilename;

        private bool isSerialTest;

        private string requestSubject;
        private string replySubject;

        private NetTransport transport;
        private string inbox;
        private Queue queue;

        private Listener replyListener;
        private Dispatcher replyDispatcher;

        private int received;
        private double discarded;
        private double sampled;

        private double minRtt;
        private double maxRtt;
        private double sum;

        private int above1msCount;

        private System.Collections.Generic.Queue<double> measurementsQueue;

        long testStartTimestamp;


        private static string formatDouble(double d)
        {
            return String.Format("{0:0.00}", d);
        }


        public Client(string[] args)
        {
            this.service = null;
            this.network = null;
            this.daemon = null;

            this.reliability = RVLat.DEFAULT_RELIABILITY;
            this.debug = RVLat.DEFAULT_DEBUG;

            this.useInboxes = false;

            this.messageCount = RVLat.DEFAULT_MESSAGE_COUNT;
            this.packAndUnpack = false;
            this.batchSize = RVLat.DEFAULT_BATCH_SIZE;
            this.batchInterval = RVLat.DEFAULT_BATCH_INTERVAL;
            this.yieldAfterEachSend = false;
            this.useDispose = false;

            this.outlierThreshold = Double.MaxValue;
            this.timeLimit = 0;

            this.logFilename = RVLat.DEFAULT_LOG_FILENAME;

            this.isSerialTest = true;

            parseParameters(args);
        }

        private void parseParameters(string[] args)
        {
            int i = 1;

            for (i = 1; i < args.Length; i++)
            {
                if ("-service".Equals(args[i]))
                {
                    if (++i >= args.Length)
                        RVLat.Usage();

                    this.service = args[i];
                }
                else if ("-network".Equals(args[i]))
                {
                    if (++i >= args.Length)
                        RVLat.Usage();

                    this.network = args[i];
                }
                else if ("-daemon".Equals(args[i]))
                {
                    if (++i >= args.Length)
                        RVLat.Usage();

                    this.daemon = args[i];
                }
                else if ("-reliability".Equals(args[i]))
                {
                    if (++i >= args.Length)
                        RVLat.Usage();

                    this.reliability = args[i];
                }
                else if ("-debug".Equals(args[i]))
                {
                    if (++i >= args.Length)
                        RVLat.Usage();

                    this.debug = args[i];
                }
                else if ("-inbox".Equals(args[i]))
                {
                    this.useInboxes = true;
                }
                else if ("-messages".Equals(args[i]))
                {
                    if (++i >= args.Length)
                        RVLat.Usage();

                    this.messageCount = Convert.ToInt32(args[i]);
                }
                else if ("-pack".Equals(args[i]))
                {
                    this.packAndUnpack = true;
                }
                else if ("-batch".Equals(args[i]))
                {
                    if (++i >= args.Length)
                        RVLat.Usage();

                    this.batchSize = Convert.ToInt32(args[i]);

                    this.isSerialTest = false;
                }
                else if ("-interval".Equals(args[i]))
                {
                    if (++i >= args.Length)
                        RVLat.Usage();

                    this.batchInterval = Convert.ToDouble(args[i]);
                }
                else if ("-yield".Equals(args[i]))
                {
                    this.yieldAfterEachSend = true;
                }
                else if ("-dispose".Equals(args[i]))
                {
                    this.useDispose = true;
                }
                else if ("-spikes".Equals(args[i]))
                {
                    if (++i >= args.Length)
                        RVLat.Usage();

                    this.outlierThreshold = Convert.ToDouble(args[i]);
                }
                else if ("-time".Equals(args[i]))
                {
                    if (++i >= args.Length)
                        RVLat.Usage();

                    this.timeLimit = Convert.ToDouble(args[i]);

                    if (this.timeLimit <= 0.0)
                    {
                        Console.WriteLine("-time must be > 0.");

                        System.Environment.Exit(1);
                    }
                }
                else if ("-w".Equals(args[i]))
                {
                    if (++i >= args.Length)
                        RVLat.Usage();

                    this.logFilename = args[i];
                }
                else if ("-help".Equals(args[i])
                         || "-h".Equals(args[i]))
                {
                    RVLat.Usage();
                }
                else
                {
                    Console.WriteLine("Unknown option: " + args[i]);

                    RVLat.Usage();
                }
            }
        }

        void initialize()
        {
            uint argc = 0;
            string[] argv = new string[4];

            if (this.reliability != null)
            {
                argv[argc++] = "-reliability";
                argv[argc++] = this.reliability;
            }

            if (this.debug != null)
            {
                argv[argc++] = "-debug";
                argv[argc++] = this.debug;
            }

            if (argc > 0)
            {
                // If argc > 0, IPM mode is implicitly requested.

                Status status = TIBCO.Rendezvous.Environment.SetRVParameters(argv);
                if (status == Status.OK)
                {
                    Console.WriteLine("Successfully set IPM parameters.\n");
                }
                else
                {
                    Console.WriteLine("Failed to set IPM parameters.\n");

                    RVLat.Usage();
                }
            }

            this.requestSubject = RVLat.DEFAULT_REQUEST_SUBJECT;
            this.replySubject = RVLat.DEFAULT_REPLY_SUBJECT;

            try
            {
                TIBCO.Rendezvous.Environment.Open();
            }
            catch (RendezvousException exception)
            {
                Console.Error.WriteLine(exception.StackTrace);

                System.Environment.Exit(1);
            }

            try
            {
                this.transport = new NetTransport(this.service, this.network, this.daemon);

                this.inbox = this.transport.CreateInbox();

                this.queue = new Queue();

                this.replyListener = null;
                if (this.isSerialTest)
                {
                    this.replyDispatcher = null;
                }
                else
                {
                    this.replyDispatcher = new Dispatcher(this.queue);
                }
            }
            catch (RendezvousException exception)
            {
                Console.Error.WriteLine(exception.ToString());

                System.Environment.Exit(1);
            }

            this.received = 0;
            this.discarded = 0d;
            this.sampled = 0d;

            this.minRtt = 0;
            this.maxRtt = 0;
            this.sum = 0;

            this.above1msCount = 0;

            this.measurementsQueue = new System.Collections.Generic.Queue<double>(RVLat.DEFAULT_MESSAGE_COUNT);
        }

        private void locateServer()
        {
            Console.WriteLine("Locating server ...");

            Message serverRequest = new Message();

            serverRequest.SendSubject = RVLat.DEFAULT_INITIALIZATION_SUBJECT;

            serverRequest.AddField(RVLat.DEFAULT_USE_INBOXES_FIELD_NAME, this.useInboxes);

            Message serverReply = null;

            try
            {
                serverReply = this.transport.SendRequest(serverRequest, RVLat.DEFAULT_SEARCH_TIMEOUT);
                if (serverReply == null)
                {
                    Console.WriteLine("No response from server!");

                    System.Environment.Exit(1);
                }
            }
            catch (RendezvousException exception)
            {
                Console.Error.WriteLine(exception.StackTrace);

                System.Environment.Exit(1);
            }

            if (this.useInboxes)
            {
                this.requestSubject = serverReply.ReplySubject;
                this.replySubject = this.inbox;
            }

            Console.WriteLine("Server Found!");
        }

        private void computeRttStatistics(long sentTimestamp, long receivedTimestamp)
        {
            double rtt = MyStopwatch.TimeSpanInMilliseconds(sentTimestamp, receivedTimestamp);

            if (rtt > 0.0 && rtt < this.outlierThreshold)
            {
                this.measurementsQueue.Enqueue(rtt);

                if (this.minRtt == 0.0 || rtt < this.minRtt)
                    this.minRtt = rtt;

                if (rtt > this.maxRtt)
                    this.maxRtt = rtt;

                this.sum += rtt;

                if (rtt > 1.0)
                    this.above1msCount++;

                this.sampled++;
            }
            else
            {
                this.discarded++;
            }
        }

        // If you pass "-pack" on the command line, field extraction can be included in timimg results.
        // This "unpack" method is used to extract a payload from replies.
        // If you modify this method, also change the "pack" method.
        private void unpack(Message message)
        {
            message.GetField("", 1);
        }

        private void handleReply(object listener, MessageReceivedEventArgs messageReceivedEventArgs)
        {
            if (this.packAndUnpack)
            {
                unpack(messageReceivedEventArgs.Message);
            }

            long receivedTimestamp = MyStopwatch.GetTimestamp();
            long sentTimestamp = (long)messageReceivedEventArgs.Message.GetField("t0").Value;

            if (this.useDispose)
            {
                messageReceivedEventArgs.Message.Dispose();
            }

            this.received++;

            // First 10 messages are warm-up messages, they cause dynamic loading of system libraries and JIT optimization.
            if (this.received > 10)
            {
                computeRttStatistics(sentTimestamp, receivedTimestamp);
            }

            if (!this.isSerialTest && this.received == this.messageCount)
            {
                this.replyDispatcher.Destroy();
            }
        }

        private void setUpListener()
        {
            this.replyListener = new Listener(this.queue, this.transport, this.replySubject, null);
            this.replyListener.MessageReceived += new MessageReceivedEventHandler(this.handleReply);
        }

        // If you pass "-pack" on the command line, field extraction can be included in timimg results.
        // This "pack" method is used to add a payload to requests.
        // If you modify this method, also change the "unpack" method.
        private void pack(Message message)
        {
            message.AddField("", new byte[128], 1);
        }

        private void printTimeAndRate()
        {
            long testStopTimestamp = MyStopwatch.GetTimestamp();
            double totalExecutionTime = MyStopwatch.TimeSpanInMilliseconds(this.testStartTimestamp, testStopTimestamp);

            Console.WriteLine(this.received
                + " count, "
                + formatDouble(totalExecutionTime / 1000d)
                + " s elapsed, "
                + formatDouble((this.received / (totalExecutionTime / 1000d)))
                + " msgs/s\n");
        }

        private void send()
        {

            try
            {
                Message[] data = new Message[this.batchSize];
                for (int i = 0; i < this.batchSize; i++)
                {
                    data[i] = new Message();
                    data[i].SendSubject = this.requestSubject;
                    data[i].ReplySubject = this.replySubject;
                    data[i].UpdateField("sqn", (int)0);
                    data[i].UpdateField("t0", (long)0);

                    if (this.packAndUnpack)
                    {
                        pack(data[i]);
                    }
                }

                int batch = 0;
                int remaining = this.messageCount;
                int sequence = 0;
                this.testStartTimestamp = MyStopwatch.GetTimestamp();

                Console.WriteLine("Beginning latency test ...\n");

                while (true)
                {
                    if (this.isSerialTest || this.timeLimit > 0.0)
                    {
                        batch = this.batchSize;
                    }
                    else
                    {
                        batch = (remaining < this.batchSize) ? remaining : this.batchSize;
                    }

                    for (int i = 0; i < batch; ++i)
                    {
                        sequence++;

                        data[i].UpdateField("sqn", sequence);
                        data[i].UpdateField("t0", MyStopwatch.GetTimestamp());

                        this.transport.Send(data[i]);

                        if (!this.isSerialTest && this.yieldAfterEachSend)
                        {
                            Thread.Sleep(0);
                        }
                    }

                    if (this.isSerialTest)
                    {
                        this.queue.Dispatch();
                    }

                    remaining -= batch;

                    long checkpointTimestamp = MyStopwatch.GetTimestamp();

                    if ((this.timeLimit == 0.0 && remaining == 0)
                        || (this.timeLimit > 0.0 && MyStopwatch.TimeSpanInMilliseconds(this.testStartTimestamp, checkpointTimestamp) > this.timeLimit))
                    {

                        break;
                    }

                    if (!this.isSerialTest)
                    {
                        Thread.Sleep((int)Math.Round(this.batchInterval * 1000));
                    }
                }

                if (!this.isSerialTest)
                {
                    this.replyDispatcher.Join();
                }
            }
            catch (RendezvousException exception)
            {
                Console.Error.WriteLine(exception.StackTrace);

                System.Environment.Exit(1);
            }

            printTimeAndRate();
        }

        private void logResults()
        {
            TextWriter textWriter = new StreamWriter(this.logFilename);

            foreach (double rtt in this.measurementsQueue)
            {
                textWriter.WriteLine(rtt);
            }

            textWriter.Close();
        }

        private void outputSummary()
        {
            double mean = (this.sum / this.sampled);

            double sumOfSquaredDeviations = 0.0;

            for (int i = 0; i < this.measurementsQueue.Count; i++)
            {
                double rtt = this.measurementsQueue.Dequeue();

                sumOfSquaredDeviations += Math.Pow(rtt - mean, 2);
            }

            double standardDeviation = Math.Sqrt(sumOfSquaredDeviations / this.sampled);

            string summary = Client.formatDouble(this.maxRtt) + " ms max, "
                                + Client.formatDouble(this.minRtt) + " min, "
                                + Client.formatDouble(mean) + " mean, "
                                + Client.formatDouble(standardDeviation) + " standard deviation, "
                                + this.above1msCount + " > 1ms, "
                                + this.sampled + " sampled, " + this.discarded + " discarded";

            Console.WriteLine("Summary: ");
            Console.WriteLine(summary);
        }

        public static void execute(string[] args)
        {

            // This is a mask. 0x0001 = processor 0, 0x0002 = processor 1, 0x0003 = processor 0 and 1
            // Process.GetCurrentProcess().ProcessorAffinity = (System.IntPtr)1;
            // Console.WriteLine("New ProcessorAffinity: {0}", Process.GetCurrentProcess().ProcessorAffinity);

            Console.WriteLine("Requesting low latency Garbage Collection mode...");
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;

            Console.WriteLine("Preloading assemblies...");
            Preloader.PreloadAssemblies();

            Console.Out.Flush();

            Client client = new Client(args);

            client.initialize();
            client.locateServer();
            client.setUpListener();
            client.send();
            client.logResults();
            client.outputSummary();

            Console.Out.Flush();
        }

    }

}
