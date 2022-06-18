/*
 * Copyright (c) 1998-$Date: 2016-12-13 12:47:31 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
 * All Rights Reserved. Confidential & Proprietary.
 * TIB/Rendezvous is protected under US Patent No. 5,187,787.
 * For more information, please contact:
 * TIBCO Software Inc., Palo Alto, California, USA
 *
 */

/*
 * tibrvftmon.java - example TIB/Rendezvous fault tolerant group
 *                   monitor
 *
 * This program monitors the fault tolerant group TIBRVFT_TIME_EXAMPLE,
 * the group established by the tibrvfttime timestamp message sending
 * program.   It will report a change in the number of active members
 * of that group.
 *
 * The tibrvftmon program must use the default communication
 * parameters.
 */

import java.util.*;
import com.tibco.tibrv.*;

public class tibrvftmon implements TibrvFtMonitorCallback
{

    String  ftgroupName = "TIBRVFT_TIME_EXAMPLE";

    TibrvTransport  transport;
    TibrvFtMonitor  ftMonitor;
    double          lostInterval = 4.8;     // matches tibrvfttime
    static int      oldNumActive = 0;
    String          service = null;         // service "Rendezvous"
    String          network = null;         // the result of gethostname
    String          daemon = null;          // daemon "7500"

    public tibrvftmon(String args[])
    {
        // parse arguments for possible optional
        // parameters.
        get_InitParams(args);

        // open Tibrv in native implementation
        try
        {
            Tibrv.open(Tibrv.IMPL_NATIVE);
        }
        catch (TibrvException e)
        {
            System.err.println("Failed to open Tibrv in native implementation:");
            e.printStackTrace();
            System.exit(0);
        }

        // Initialize transport
        try
        {
            transport = new TibrvRvdTransport(service, network, daemon);
        }
        catch (TibrvException e)
        {
            System.err.println("Failed to create TibrvRvdTransport:");
            e.printStackTrace();
            System.exit(0);
        }

        // Set up the monitoring of the TIBRVFT_TIME_EXAMPLE group.
        try {
            ftMonitor = new TibrvFtMonitor(Tibrv.defaultQueue(),
                              this,
                              transport,
                              ftgroupName,
                              lostInterval,
                              null);
            }
            catch (TibrvException e)
            {
                System.err.println("Exception starting ft group monitor:");
                e.printStackTrace();
                System.exit(0);
            }

        System.err.println("tibrvftmon: Waiting for group information...");

        // dispatch Tibrv events
        while(true)
        {
            try
            {
                Tibrv.defaultQueue().dispatch();
            }
            catch (TibrvException e)
            {
                System.err.println("Exception dispatching default queue:");
                e.printStackTrace();
                System.exit(0);
            }
            catch(InterruptedException ie)
            {
                System.exit(0);
            }
        }
    }

/*
 * Fault tolerance monitor callback called when TIBRVFT detects a
 * change in the number of active members in group TIBRVFT_TIME_EXAMPLE.
 */

    public void onFtMonitor(TibrvFtMonitor ftMonitor, String ftgroupName, int numActive)
    {
    //  static int oldNumActive = 0;
        System.out.println("Group ["+ftgroupName+"]: has "+numActive+" members (after "+
                          ((oldNumActive > numActive)?"one deactivated":"one activated")+").");
        oldNumActive = numActive;

    } // onFtMonitor

    // print usage information and quit
    void usage()
    {
        System.err.println("Usage: java tibrvftmon [-service service] [-network network]");
        System.err.println("                       [-daemon daemon] ");
        System.exit(-1);
    }

    void get_InitParams(String[] args)
    {
        int i = 0;
        while (i < args.length - 1 && args[i].startsWith("-"))
        {
            if (args[i].equals("-service"))
            {
                service = args[i + 1];
                i += 2;
            }
            else
            if (args[i].equals("-network"))
            {
                network = args[i + 1];
                i += 2;
            }
            else
            if (args[i].equals("-daemon"))
            {
                daemon = args[i + 1];
                i += 2;
            }
            else
                usage();
        }
    }

    public static void main(String args[])
    {
        new tibrvftmon(args);
    }

}
