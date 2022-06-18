/*
 * Copyright (c) 1998-$Date: 2016-12-13 12:47:31 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
 * All Rights Reserved. Confidential & Proprietary.
 * TIB/Rendezvous is protected under US Patent No. 5,187,787.
 * For more information, please contact:
 * TIBCO Software Inc., Palo Alto, California, USA
 *
 */

/*
 * tibrvsend - sample Rendezvous message publisher
 *
 * This program publishes one or more string messages on a specified
 * subject.  Both the subject and the message(s) must be supplied as
 * command parameters.  Message(s) with embedded spaces should be quoted.
 * A field named "DATA" will be created to hold the string in each
 * message.
 *
 * Optionally the user may specify communication parameters for
 * tibrvTransport_Create.  If none are specified, default values
 * are used.  For information on default values for these parameters,
 * please see the TIBCO/Rendezvous Concepts manual.
 *
 *
 * Normally a listener such as tibrvlisten should be started first.
 *
 * Examples:
 *
 *  Publish two messages on subject a.b.c and default parameters:
 *   java tibrvsend a.b.c "This is my first message" "This is my second message"
 *
 *  Publish a message on subject a.b.c using port 7566:
 *   java tibrvsend -service 7566 a.b.c message
 */

import java.util.*;
import com.tibco.tibrv.*;

public class tibrvsend
{

    String service = null;
    String network = null;
    String daemon  = null;

    String FIELD_NAME = "DATA";

    public tibrvsend(String args[])
    {
        // parse arguments for possible optional
        // parameters. These must precede the subject
        // and message strings
        int i = get_InitParams(args);

        // we must have at least one subject and one message
        if (i > args.length-2)
            usage();

        // open Tibrv in native implementation
        try
        {
            if (Tibrv.isIPM()) {
                /*
                 * The Rendezvous IPM library is only supported by tibrvnative.jar
                 *
                 * Prior to using the Rendezvous IPM library please read the
                 * appropriate sections of user guide to determine if IPM is the
                 * correct choice for your application; it is likely not.
                 *
                 * To use IPM in Java on supported platforms, first make sure
                 * the IPM version of the Rendezvous shared library is in your
                 * library path (ahead of the standard version of the Rendezvous
                 * library).
                 *
                 * The IPM shared library can be found in $TIBRV_HOME/lib/ipm (or
                 * $TIBRV_HOME/bin/ipm for the Windows DLL)
                 *
                 * To configure IPM you can do one of the following:
                 *
                 * 1) Nothing, and accept the default IPM RV parameter values.
                 *
                 * 2) Place a file named "tibrvipm.cfg" in your PATH, and have
                 * IPM automatically read in configuration values.
                 *
                 * 3) Call Tibrv.setRVParameters, prior to Tibrv.open:
                 *
                 *   String rvParams[] = {"-reliability", "3", "-reuse-port", "30000"};
                 *   Tibrv.setRVParameters(rvParams);
                 *   Tibrv.open();
                 *
                 * 4) Call Tibrv.open(<pathname>), and have IPM read in the
                 * configuration values:
                 *
                 *   String cfgfile = "/var/tmp/mycfgfile"
                 *   Tibrv.open(cfgfile);
                 *
                 * An example configuration file, "tibrvipm.cfg", can be found in the
                 * "$TIBRV_HOME/examples/IPM directory" of the Rendezvous installation.
                 */
                Tibrv.open("./tibrvipm.cfg");
            } else {
                Tibrv.open(Tibrv.IMPL_NATIVE);
            }
        }
        catch (TibrvException e)
        {
            System.err.println("Failed to open Tibrv in native implementation:");
            e.printStackTrace();
            System.exit(0);
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
            System.exit(0);
        }

        // Create the message
        TibrvMsg msg = new TibrvMsg();

        // Set send subject into the message
        try
        {
            msg.setSendSubject(args[i++]);
        }
        catch (TibrvException e) {
            System.err.println("Failed to set send subject:");
            e.printStackTrace();
            System.exit(0);
        }

        try
        {
            // Send one message for each parameter
            while (i < args.length)
            {
                System.out.println("Publishing: subject="+msg.getSendSubject()+
                            " \""+args[i]+"\"");
                msg.update(FIELD_NAME,args[i]);
                transport.send(msg);
                i++;
            }
        }
        catch (TibrvException e)
        {
            System.err.println("Error sending a message:");
            e.printStackTrace();
            System.exit(0);
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
            System.exit(0);
        }

    }

    // print usage information and quit
    void usage()
    {
        System.err.println("Usage: java tibrvsend [-service service] [-network network]");
        System.err.println("            [-daemon daemon] <subject> <messages>");
        System.exit(-1);
    }

    int get_InitParams(String[] args)
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

    public static void main(String args[])
    {
        new tibrvsend(args);
    }

}
