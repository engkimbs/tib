/*
 * Copyright (c) 1998-$Date: 2016-12-13 12:47:31 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
 * All Rights Reserved. Confidential & Proprietary.
 * TIB/Rendezvous is protected under US Patent No. 5,187,787.
 * For more information, please contact:
 * TIBCO Software Inc., Palo Alto, California, USA
 *
 */

/*
 *  TibrvVectorListenerTester.java
 *
 * This program and TibrvVectorListener demonstrate the behavior of the vector listener.
 * As such it is not an example of the best way to write such applications.
 *
 * The following scenario is created:
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
*/

import com.tibco.tibrv.*;

public class TibrvVectorListenerTester {
    String      service = null;
    String      network = null;
    String      daemon  = null;
    int         nrBatches = 1;
    int         sent = 1;
    TibrvMsg    msg[] = new TibrvMsg[90];
    String      FIELD_NAME = "DATA";

    public TibrvVectorListenerTester(String args[])
    {
        // parse arguments for possible optional. These must precede the subject
        // and message strings

        int i = getInitParams(args);
        int index = 0;

        if (i > args.length)
        {
            usage();
        }

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
        }
        catch (TibrvException e)
        {
            System.err.println("Failed to create TibrvRvdTransport:");
            e.printStackTrace();
            System.exit(1);
        }

        // Create the message


        for(i=0;i < 90;i++)
        {
            msg[i] = new TibrvMsg();
            if(msg[i] == null)
                System.exit(1);
        }

        // Set send subject into the message
        try
        {
            for(i=0;i < 90;i++)
            {
                /* set the following subjects
                 * a.2, a.3, b.1, b.2, b.3, a.1, a.4, b.4, c.1
                 */
                index = i%9;
                switch(index)
                {
                case 0:
                    msg[i].setSendSubject("a.2");
                    break;
                case 1:
                    msg[i].setSendSubject( "a.3");
                    break;
                case 2:
                    msg[i].setSendSubject( "b.1");
                    break;
                case 3:
                    msg[i].setSendSubject( "b.2");
                    break;
                case 4:
                    msg[i].setSendSubject( "b.3");
                    break;
                case 5:
                    msg[i].setSendSubject( "a.1");
                    break;
                case 6:
                    msg[i].setSendSubject( "a.4");
                    break;
                case 7:
                    msg[i].setSendSubject( "b.4");
                    break;
                case 8:
                    msg[i].setSendSubject( "c.1");
                    break;
                default:
                    msg[i].setSendSubject( "hello");
                    break;
                }

                msg[i].update(FIELD_NAME,"DATA");
            }
        }
        catch (TibrvException e)
        {
            System.err.println("Failed to set send subject:");
            e.printStackTrace();
            System.exit(1);
        }

        System.out.println("Will publish  " + nrBatches +
                           " batches of messages [90 messages each batch]");
        System.out.println("Messages are arranged in follwing order based on their subjects:\n" +
                           "a.2, a.3, b.1, b.2, b.3, a.1, a.4, b.4, c.1");
        while(sent <= nrBatches)
        {
            try
            {
                System.out.println("Publishing batch number "+sent);
                transport.send(msg);
                sent++;
            }
            catch(Exception ex)
            {
                ex.printStackTrace();
                System.exit(1);
            }
        }

        // Close Tibrv, it will cleanup all underlying memory, destroy
        // transport and guarantee delivery.

        try
        {
            Tibrv.close();
        }
        catch(TibrvException e)
        {
            System.err.println("Exception dispatching default queue:");
            e.printStackTrace();
            System.exit(1);
        }
    }

    // print usage information and quit
    void usage()
    {
        System.err.println("Usage: java TibrvVectorListenerTester [-service service] [-network network]");
        System.err.println("            [-daemon daemon] [-batches  number of batches]");
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
            else if (args[i].equals("-network"))
            {
                network = args[i+1];
                i += 2;
            }
            else if (args[i].equals("-daemon"))
            {
                daemon = args[i+1];
                i += 2;
            }
            else if (args[i].equals("-batches"))
            {
                try
                {
                    this.nrBatches = new Integer(args[i+1]).intValue();
                    i += 2;
                }
                catch(Exception ex)
                {
                    System.err.println("Unable to parse "+args[i+1]+" into an int");
                    System.exit(1);
                }
            }
            else
                usage();
        }
        return i;
    }

    public static void main(String args[])
    {
        new TibrvVectorListenerTester(args);
    }
}
