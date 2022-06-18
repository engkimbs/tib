/*
 * Copyright (c) 1998-$Date: 2016-12-13 12:47:31 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
 * All Rights Reserved. Confidential & Proprietary.
 * TIB/Rendezvous is protected under US Patent No. 5,187,787.
 * For more information, please contact:
 * TIBCO Software Inc., Palo Alto, California, USA
 *
 */

import com.tibco.tibrv.*;

public class VectoredDispatcher extends TibrvVectorCallback{
	  String subject   = "dispatchers.test";  // test subject
	    long   startTime = 0;                   // the time we start

	    int processedMessageCount = 0;          // count of processed messages

	    static final int TOTAL_MESSAGES = 10;   // total number of messages

	    public VectoredDispatcher()
	    {
	    }

	    public void execute()
	    {
	        try
	        {
	            // Open Tibrv environment.
	            // This sample does not depend on the native implementation.
	            // However, if you want to use the native implementation,
	            // change the following line to use Tibrv.IMPL_NATIVE
	            Tibrv.open(Tibrv.IMPL_NATIVE);

	            // Get process transport
	            TibrvTransport transport = Tibrv.processTransport();

	            // Create the queue
	            TibrvQueue queue = new TibrvQueue();

	            // Create listener
	            new TibrvVectorListener(queue,this,transport,subject,null);

	            // Prepare the message
	            TibrvMsg msg = new TibrvMsg();
	            msg.setSendSubject(subject);

	            // Get start time
	            startTime = System.currentTimeMillis();

	            // Create two dispatchers with 3 seconds timeout
	            // so they quit when all messages are sent.
	            TibrvDispatcher dispatcher1 = new TibrvDispatcher("Dispatcher-1",queue,3);
	            TibrvDispatcher dispatcher2 = new TibrvDispatcher("Dispatcher-2",queue,3);

	            // We use this to track the message number
	            int msgIndex = 0;

	            System.err.println("Started publishing messages at "+
	                    +(System.currentTimeMillis()-startTime)/1000+" seconds");

	            // Start publishing two messages at a time
	            // every second, total of TOTAL_MESSAGES messages
	            for (int i=0; i<TOTAL_MESSAGES/2; i++)
	            {
	                // Publish 2 messages
	                for (int j=0; j<2; j++)
	                {
	                    msgIndex++;
	                    msg.update("field","value-"+msgIndex);
	                    transport.send(msg);
	                }

	                // Wait for 1 second
	                try
	                {
	                    Thread.sleep(1000);
	                }
	                catch(InterruptedException e){}
	            }

	            System.err.println("Stopped publishing messages at "+
	                    +(System.currentTimeMillis()-startTime)/1000+" seconds");

	            // Wait until dispatchers process all messages
	            // and exit after the timeout
	            try
	            {
	                dispatcher1.join();
	                dispatcher2.join();
	            }
	            catch(InterruptedException e)
	            {
	            }

	            // Close Tibrv
	            Tibrv.close();
	        }
	        catch (TibrvException rve)
	        {
	            // this program does not use the network
	            // and supposedly should never fail.
	            rve.printStackTrace();
	            System.exit(0);
	        }

	    }

	    // Message callback
	    public void onMsgs( TibrvMsg msgs[])
	    {
	    	System.err.println(Thread.currentThread().getName()+ "processing  messages");
	        // print which dispatcher got the message
	    	for(int i = 0; i< msgs.length;i++)
	        System.err.println(
	                " processing messages "+msgs[i]+
	                " at "+(System.currentTimeMillis()-startTime)/1000+" seconds");

	        // imitate message processing takes 1 second
	        try
	        {
	            Thread.sleep(1000);
	        }
	        catch(InterruptedException e){}

	        processedMessageCount =  processedMessageCount+ msgs.length;

	        // report when we done processing all TOTAL_MESSAGES messages
	        if (processedMessageCount == TOTAL_MESSAGES)
	            System.err.println("Processed all messages in "+
	                +(System.currentTimeMillis()-startTime)/1000+" seconds");
	    }

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		new VectoredDispatcher().execute();

	}

}
