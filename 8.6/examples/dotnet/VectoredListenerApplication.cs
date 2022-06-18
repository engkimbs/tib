/// Copyright (c) 1998-$Date: 2016-12-13 12:47:31 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
/// All Rights Reserved. Confidential & Proprietary.
/// TIB/Rendezvous is protected under US Patent No. 5,187,787.
/// For more information, please contact:
/// TIBCO Software Inc., Palo Alto, California, USA
using System;
using System.Text;

namespace TIBCO.Rendezvous.Examples
{
    /// <summary>
    /// Generic Rendezvous subscriber.
    /// This program listens for subject a.>  b.> c.> and a.1
    /// Message(s) received are printed. Some platforms require proper quoting of the
    /// arguments to prevent the command line processor from modifying the command arguments.
    /// The user may terminate the program by typing Control-C. Optionally the user may specify
    /// communication parameters for transport creation.
    /// If none are specified the following defaults are used:
    /// service     "rendezvous" or "7500/udp"
    /// network     the result of gethostname
    /// daemon      "tcp:7500"
    /// 
    /// Examples:
    /// 
    /// 
    /// Listen to messages published on subject above mentioned subject using port 7566:
    /// RendezvousVectorListener -service 7566 
    /// </summary>
    class VectoredListenerApplication
    {
        static string service = null;
        static string network = null;
        static string daemon = null;
        static VectorListener vectorListener1 = null;
        static VectorListener vectorListener2 = null;
        static VectorListener vectorListener3 = null;
        static Listener simpleListener = null;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main(string[] arguments)
        {
            InitializeParameters(arguments);

            try
            {
                TIBCO.Rendezvous.Environment.Open();
            }
            catch (RendezvousException exception)
            {
                Console.Error.WriteLine("Failed to open Rendezvous Environment");
                Console.Error.WriteLine(exception.StackTrace);
                System.Environment.Exit(1);
            }

            // Create Network transport
            Transport transport = null;
            try
            {
                transport = new NetTransport(service, network, daemon);
            }
            catch (RendezvousException exception)
            {
                Console.Error.WriteLine("Failed to create NetTransport");
                Console.Error.WriteLine(exception.StackTrace);
                System.Environment.Exit(1);
            }

            Queue timerQueue = null;
            try
            {
                MessagesReceivedEventHandler MessagesReceived_1 = null;
                MessagesReceivedEventHandler MessagesReceived_2 = null;
                MessagesReceived_1 = new MessagesReceivedEventHandler(OnMessagesReceived1);
                MessagesReceived_2 = new MessagesReceivedEventHandler(OnMessagesReceived2);
                vectorListener1 = new VectorListener(Queue.Default,MessagesReceived_1, transport, "a.>", null);
                Console.Out.WriteLine("Listening for a.>");
                vectorListener2 = new VectorListener(Queue.Default,MessagesReceived_1, transport, "b.>", null);

                Console.Out.WriteLine("Listening for b.>");
                vectorListener3 = new VectorListener(Queue.Default,MessagesReceived_2, transport, "c.>", null);
                Console.Out.WriteLine("Listening for c.>");
                simpleListener = new Listener(Queue.Default, transport, "a.1", null);
                simpleListener.MessageReceived += new MessageReceivedEventHandler(OnMessageReceived);
                Console.Out.WriteLine("Listening for a.1");

                timerQueue = new Queue();
            }
            catch (RendezvousException exception)
            {
                Console.Error.WriteLine("Failed to create listener:");
                Console.Error.WriteLine(exception.StackTrace);
                System.Environment.Exit(1);
            }

            // dispatch Rendezvous events
            while (true)
            {
                try
                {
                    Queue.Default.Dispatch();
                    timerQueue.TimedDispatch(1.0);
                }
                catch (RendezvousException exception)
                {
                    Console.Error.WriteLine("Exception dispatching default queue:");
                    Console.Error.WriteLine(exception.StackTrace);
                    break;
                }
            }

            // Force optimizer to keep alive listeners up to this point.
            GC.KeepAlive(vectorListener1);
            GC.KeepAlive(vectorListener2);
            GC.KeepAlive(vectorListener3);
            GC.KeepAlive(simpleListener);

            TIBCO.Rendezvous.Environment.Close();
        }

        static void Usage()
        {
            Console.Out.Write("Usage: RendezvousVectorListener [-service service] [-network network]");
            Console.Out.Write("                          [-daemon daemon] ");
            System.Environment.Exit(1);
        }

        static int InitializeParameters(string[] arguments)
        {
            int i = 0;
            while (i < arguments.Length - 1 && arguments[i].StartsWith("-"))
            {
                if (arguments[i].Equals("-service"))
                {
                    service = arguments[i + 1];
                    i += 2;
                }
                else
                    if (arguments[i].Equals("-network"))
                    {
                        network = arguments[i + 1];
                        i += 2;
                    }
                    else
                        if (arguments[i].Equals("-daemon"))
                        {
                            daemon = arguments[i + 1];
                            i += 2;
                        }
            }
            return i;
        }

        static void OnMessagesReceived1( MessagesReceivedEventArgs msgsReceivedEventArgs)
        {
            Console.Out.WriteLine("vector callback 1");
            Message[] message = msgsReceivedEventArgs.Messages;
            for (int i = 0; i < message.Length; i++)
            {
                object objListener = message[i].GetSource();

                Console.Out.Write("--> subject={0}",
                                        message[i].SendSubject);
                if (objListener is VectorListener)
                    Console.Out.WriteLine("  VectorListener");
                else
                    Console.Out.WriteLine("  Wrong listener type!!!!!");
            }
            Console.Out.Flush();
            
        }

        static void OnMessagesReceived2( MessagesReceivedEventArgs msgsReceivedEventArgs)
        {
            Console.Out.WriteLine("vector callback 2");
            Message[] message = msgsReceivedEventArgs.Messages;
            String subj = null;
            for (int i = 0; i < message.Length; i++)
            {
                Console.Out.WriteLine("--> subject={0}", message[i].SendSubject);
                subj = message[i].SendSubject;
            }
            Console.Out.Flush();
            if (subj.Equals("c.1"))
            {
                Console.Out.WriteLine("Destroying vectorListener3");
                vectorListener3.Destroy();
            }
        }

        static void OnMessageReceived(object listener, MessageReceivedEventArgs messageReceivedEventArgs)
        {
            Console.Out.WriteLine("simple callback ");
            Message message = messageReceivedEventArgs.Message;
            Console.Out.WriteLine("--> subject={0}",message.SendSubject);
            Console.Out.Flush();
        }
    }


}
