using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;
using TIBCO.Rendezvous;
using System.Diagnostics;
using System.Runtime;

namespace RVLat
{

    class Server
    {

        private string service;
        private string network;
        private string daemon;

        private string reliability;
        private string debug;

        private bool useDispose;

        private bool useSequenceTracking;

        private string requestSubject;
        private string replySubject;

        private NetTransport transport;
        private string inbox;
        private Queue queue;

        private Listener initializer;

        private Listener requestListener;

        private int received;
        private long lastSequence;


        public Server(string[] args)
        {

            this.service = null;
            this.network = null;
            this.daemon = null;

            this.reliability = RVLat.DEFAULT_RELIABILITY;
            this.debug = RVLat.DEFAULT_DEBUG;

            this.useDispose = false;

            this.useSequenceTracking = false;

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
                else if ("-dispose".Equals(args[i]))
                {
                    this.useDispose = true;
                }
                else if ("-sequence".Equals(args[i]))
                {
                    this.useSequenceTracking = true;
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

        private void initialize()
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

                this.initializer = null;

                this.requestListener = null;
            }
            catch (RendezvousException exception)
            {
                Console.Error.WriteLine(exception.ToString());

                System.Environment.Exit(1);
            }

            this.received = 0;
            this.lastSequence = 0;
        }

        private void handleRequest(object listener, MessageReceivedEventArgs messageReceivedEventArgs)
        {

            messageReceivedEventArgs.Message.SendSubject = this.replySubject;

            this.transport.Send(messageReceivedEventArgs.Message);

            if (this.useSequenceTracking)
            {
                long sqn = messageReceivedEventArgs.Message.GetField("sqn");

                if (sqn < this.lastSequence)
                {
                    Console.WriteLine("seq=" + sqn + " appears to be a duplicate message (possible rollover)\n");
                }
                else if (sqn - lastSequence > 1)
                {
                    Console.WriteLine("seq=" + sqn + " expecting " + (this.lastSequence + 1) + " (missing=" + (sqn - this.lastSequence - 1) + ")\n");
                }

                lastSequence = sqn;
            }

            if (this.useDispose)
            {
                messageReceivedEventArgs.Message.Dispose();
            }

            this.received++;
        }

        private void handleInitializationMessage(object listener, MessageReceivedEventArgs messageReceivedEventArgs)
        {

            Console.WriteLine("Run started...\n");

            this.received = 0;
            this.lastSequence = 0;

            if (this.requestListener != null)
            {
                this.requestListener.Destroy();
            }

            this.requestSubject = RVLat.DEFAULT_REQUEST_SUBJECT;

            bool useInboxes = messageReceivedEventArgs.Message.GetField(RVLat.DEFAULT_USE_INBOXES_FIELD_NAME);

            if (useInboxes)
            {
                this.requestSubject = this.inbox;
            }

            this.requestListener = new Listener(this.queue, this.transport, this.requestSubject, null);
            this.requestListener.MessageReceived += new MessageReceivedEventHandler(this.handleRequest);

            Message reply = new Message();
            reply.ReplySubject = this.inbox;

            this.transport.SendReply(reply, messageReceivedEventArgs.Message);
        }

        private void waitForClient()
        {

            this.initializer = new Listener(this.queue, this.transport, RVLat.DEFAULT_INITIALIZATION_SUBJECT, null);
            this.initializer.MessageReceived += new MessageReceivedEventHandler(this.handleInitializationMessage);

            while (true)
            {
                this.queue.Dispatch();
            }
        }

        public static void execute(string[] args)
        {

            // This is a mask. 0x0001 = processor 0, 0x0002 = processor 1, 0x0003 = processor 0 and 1
            // Process.GetCurrentProcess().ProcessorAffinity = (System.IntPtr)2;
            // Console.WriteLine("New ProcessorAffinity: {0}", Process.GetCurrentProcess().ProcessorAffinity);

            Console.WriteLine("Requesting low latency Garbage Collection mode...");
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;

            Console.WriteLine("Preloading assemblies...");
            Preloader.PreloadAssemblies();

            Console.Out.Flush();

            Server program = new Server(args);

            program.initialize();
            program.waitForClient();

            Console.Out.Flush();
        }
    }

}
