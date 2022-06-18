/*
 * Copyright (c) 1998-$Date: 2016-12-13 12:47:31 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
 * All Rights Reserved. Confidential & Proprietary.
 * TIB/Rendezvous is protected under US Patent No. 5,187,787.
 * For more information, please contact:
 * TIBCO Software Inc., Palo Alto, California, USA
 *
 */

/*
 * TibrvVectoredListener.java
 *
 * This program and TibrvVectorListenerTester demonstrate the behavior of the
 * vector listener.  As such it is not an example of the best way to write such
 * applications.
 *
 * The corresponding receiver (TibrvVectorListen) does the following:
 *  Create vector listener 1 on subject "a.>" with vectorCallback1.
 *  Create vector listener 2 on subject "b.>" with vectorCallback1.
 *  Create vector listener 3 on subject "c.>" with vectorCallback2.
 *  Create simple listener   on subject "a.1" with simpleCallback.
 *
 * Messages come in from TibrvVectorListenerTester (this program) in the following order:
 *      a.2, a.3, b.1, b.2, b.3, a.1, a.4, b.4, c.1  repeated 10 times
 *
 * The callbacks are driven as follows:
 *
 * vectorCallback1 with a vector of  a.2, a.3, b.1, b.2, b.3, a1
 *      (possible in a sigle invocation)
 *
 * simplecallback with a.1
 *
 * vectorCallback1 with a vector of -  a.4, b.4 (possible in a sigle invocation)
 *
 * vectorCallback2 with a vector of c.1
 * 
 * This illustrates that it is very likely that vectorCallback_1 would get
 * vectors containing messages combined from the first two listeners.
 *
 * We added a second queue "waitQueue" that dispatches nothing but it is used to
 * wait 1.0 sec after each dispatch on default queue, giving time for more
 * messages to arrive so we can capture the behavior described above.
 */

import java.util.*;

import com.tibco.tibrv.*;
public class TibrvVectoredListen 
{
        class Closure
        {
            String  listenerName = null;
            Closure(String name)
            {
                this.listenerName = name;
            }
        }

        class SimpleCallback implements TibrvMsgCallback
        {
            String name = null;
            SimpleCallback(String name)
            {
                this.name = name;
            }

            public void onMsg(TibrvListener listener, TibrvMsg msg)
            {
                System.out.println(name+" received msg:");
                System.out.println(" ---> subject=" + msg.getSendSubject() + " " +
                                   ((Closure)(msg.getEvent().getClosure())).listenerName);
            }
        }

        class VectorCallback extends TibrvVectorCallback
        {
            String name = null;
            VectorCallback(String name)
            {
                this.name = name;
            }

            public void onMsgs(TibrvMsg msgs[])
            {
                System.out.println(this.name +" received new vector of " +
                                   msgs.length + " messages");

                for(int i=0;i<msgs.length;i++)
                {
                    System.out.println(" ---> subject=" + msgs[i].getSendSubject() +
                                       " for " +
                                       ((Closure)(msgs[i].getEvent().getClosure())).listenerName);
                    System.out.flush();
                }
            }
        }

    String              service = null;
    String              network = null;
    String              daemon  = null;
    TibrvQueue          timerQueue = null;
    VectorCallback      vectorCallback1 = null;
    VectorCallback      vectorCallback2 = null;
    SimpleCallback      simpleCallback = null;
    TibrvVectorListener vectorListener1 = null;
    TibrvVectorListener vectorListener2 = null;
    TibrvVectorListener vectorListener3 = null;
    TibrvListener       simpleListener1 = null;

    public TibrvVectoredListen(String args[])
    {
        // parse arguments for possible optional
        // parameters. These must precede the subject
        // and message strings
        int i = getInitParams(args);

        // open Tibrv in native implementation
        try
        {
	    /*
	      When using IPM, there are 3 ways to provide configuration parameters:
	      1) Using the new Tibrv.setRVParameters API.
	      2) Calling Open with the pathname of a configuration file.
	      3) Placing a "tibrvipm.cfg" configuration file somewhere in PATH.

	      Uncomment the following line to test approach 2):
	      Tibrv.open(".\\tibrvipm.cfg");

	      NOTE: Add *only* the Rendezvous IPM jar file to your classpath to use IPM.
	    */
            Tibrv.open(Tibrv.IMPL_NATIVE);
        }
        catch (TibrvException e)
        {
            System.err.println("Failed to open Tibrv in native implementation:");
            e.printStackTrace();
            System.exit(1);
        }

        // Create RVD transport
        TibrvTransport transport = null;
        try
        {
            transport = new TibrvRvdTransport(service,network,daemon);
            timerQueue = new TibrvQueue();
        }
        catch (TibrvException e)
        {
            e.printStackTrace();
            System.exit(1);
        }

        try
        {
            vectorCallback1 = new VectorCallback("vector callback 1");
            vectorCallback2 = new VectorCallback("vector callback 2");
            simpleCallback = new SimpleCallback("simple callback");
            vectorListener1 = new TibrvVectorListener(Tibrv.defaultQueue(),
                                                      vectorCallback1,transport,
                                                      "a.>",
                                                      new Closure("vector listener 1"));

            System.out.println("Ready to receive subject a.>");
            vectorListener2 = new TibrvVectorListener(Tibrv.defaultQueue(),
                                                      vectorCallback1,transport,
                                                      "b.>",
                                                      new Closure("vector listener 2"));
            System.out.println("Ready to receive subject b.>");
            vectorListener3 = new TibrvVectorListener(Tibrv.defaultQueue(),
                                                      new VectorCallback("vector callback 2"),
                                                      transport,
                                                      "c.>",
                                                      new Closure("vector listener 3"));
            System.out.println("Ready to receive subject c.>");
            simpleListener1 = new TibrvListener(Tibrv.defaultQueue(),
                                                simpleCallback,transport,
                                                "a.1",
                                                new Closure("simple listener 1"));

            System.out.println("Ready to receive subject a.1");
        }
        catch (TibrvException e)
        {
            System.err.println("Failed to create listener:");
            e.printStackTrace();
            System.exit(1);
        }

        // dispatch Tibrv events
        while(true)
        {
            try
            {
                Tibrv.defaultQueue().dispatch();
                timerQueue.timedDispatch(1.0);
            }
            catch(TibrvException e)
            {
                System.err.println("Exception dispatching default queue:");
                e.printStackTrace();
                System.exit(1);
            }
            catch(InterruptedException ie)
            {
                ie.printStackTrace();
                System.exit(1);
            }
        }
    }


    // print usage information and quit
    void usage()
    {
        System.err.println("Usage: java tibrvvectoredlisten [-service service] [-network network]");
        System.err.println("            [-daemon daemon] ");
        System.exit(-1);
    }

    int getInitParams(String[] args)
    {
        int i=0;
        while(i < args.length-1 && args[i].startsWith("-"))
        {
            if (args[i].equals("-service"))
            {
                service = args[i+1];
                i += 2;
            }
            else
            if (args[i].equals("-network"))
            {
                network = args[i+1];
                i += 2;
            }
            else
            if (args[i].equals("-daemon"))
            {
                daemon = args[i+1];
                i += 2;
            }
            else
                usage();
        }
        return i;
    }

    public static void main(String[] args)
    {
        new TibrvVectoredListen(args);
    }

}
