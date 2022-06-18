/*
 * Copyright (c) 1998-$Date: 2018-08-28 12:33:25 -0700 (Tue, 28 Aug 2018) $ TIBCO Software Inc.
 * All Rights Reserved. Confidential & Proprietary.
 * TIB/Rendezvous is protected under US Patent No. 5,187,787.
 * For more information, please contact:
 * TIBCO Software Inc., Palo Alto, California, USA
 *
 */

/*
 * This program and tibrvvectorlistenertester show how vectorlistener behaves
 * NOT how to write good RV based applications
 * It also shows how to use  the newly introduced functions tibrvMsg_GetClosure() and tibrvMsg_GetEventId()
 * The following scenario is created:
 *  Create vector listener 1 on subject "a.>" with vectorCb1.
 *  Create vector listener 2 on subject "b.>" with vectorCb1.
 *  Create vector listener 3 on subject "c.>" with vectorCb2.
 *  Create simple listener   on subject "a.1" with simpleCb1.
 *  Messages come in from tibrvvectorlistenertester in the following order:
 *          a.2, a.3, b.1, b.2, b.3, a.1, a.4, b.4, c.1  repeatedly 10 times
 * The callbacks are  driven as follows:
 * vectorCb1 with a vector of  a.2, a.3, b.1, b.2, b.3
 * -    in a sigle invokation if they are all present in that order in the
 * -    queue at the time of executing dispatch function
 * simpleCb1 with a.1
 * vectorCb1 with a vector of -  a.4, b.4 (possible in a sigle invokation same as above)
 * vectorCb2 with a vector of c.1
 * Main point with this it is very likely that vectorCb1 gets  vectors containing messages
 * combined from the first two listeners.
 * We added a second queue "waitQueue" that dispatches nothing but it is used to wait  1.0 sec
 * after each dispatch on default queue, giving time for more messages to arrive  sowecan capture
 * the behaviour described above
 */

#include <stdlib.h>
#include <stdio.h>
#include <signal.h>
#include <string.h>
#include <time.h>

#include "tibrv/tibrvcpp.h"

/*********************************************************************/
/* Transport parameters                                              */
/*********************************************************************/
class Closure
{
    public:
        char listenerName[24];
        Closure(const char* name)
        {
            strncpy(listenerName,name,23);
        }

};

char* serviceStr = NULL;
char* networkStr = NULL;
char* daemonStr  = NULL;

/*********************************************************************/
/* usage:                                                            */
/*         print usage information and quit                          */
/*********************************************************************/
void usage()
{
    fprintf(stderr,"tibrvmixedlisteners [-service service] [-network network] \n");
    fprintf(stderr,"            [-daemon daemon] \n");

    exit(1);
}

/*********************************************************************/
/* getTransportParams:                                               */
/*         Get from the command line the parameters that should be   */
/*         passed to TibrvNetTransport.create(). Returns the index   */
/*         for where any additional parameters can be found.         */
/*********************************************************************/
int
getTransportParams(int argc, char* argv[])
{
    int i=1;  // skip program name

    while (i+2 <= argc && *argv[i] == '-')
    {
        if (strcmp(argv[i], "-service") == 0)
        {
            serviceStr = argv[i+1];
            i+=2;
        }
        else
        if (strcmp(argv[i], "-network") == 0)
        {
            networkStr = argv[i+1];
            i+=2;
        }
        else
        if (strcmp(argv[i], "-daemon") == 0)
        {
            daemonStr = argv[i+1];
            i+=2;
        }
        else
            usage();
    }

    return i;
}

/*********************************************************************/
/* Message callback class                                            */
/*********************************************************************/
class SimpleCallback : public TibrvMsgCallback
{
    public:

        void onMsg(TibrvListener* listener, TibrvMsg& msg)
        {
            const char* sendSubject  = NULL;
            const char* msgString    = NULL;
            printf("Simple callback on listener %p: \n", (void*)listener);
            msg.getSendSubject(sendSubject);
            TibrvEvent* eventPtr = msg.getEvent();
            msg.convertToString(msgString);
            printf("    subject = %s listener id = %d\n",sendSubject,eventPtr->getHandle());
            fflush(stdout);
        }

};

class VectorCallback : public TibrvVectorCallback
{
    public:

        void onMsgs(TibrvMsg* messages[], tibrv_u32 numMessages)
        {
            tibrv_u32	i			 = 0;
            const char* sendSubject  = NULL;
            const char* msgString    = NULL;

            printf("Vector callback received %d messages\n",numMessages);

            for(i=0;i<numMessages;i++)
            {
                messages[i]->getSendSubject(sendSubject);
                messages[i]->convertToString(msgString);
                TibrvEvent* eventPtr = messages[i]->getEvent();
                Closure*   closurePtr = (Closure*)eventPtr->getClosure();
                printf("\t subject=%s, %s listener id = %d\n",sendSubject,
                            closurePtr->listenerName,
                            eventPtr->getHandle());

                fflush(stdout);

            }
        }
};


int main(int argc, char** argv)
{
    TibrvStatus status;
    /*
     * create two vector callbacks
     */
    VectorCallback* vectorCb1 = new VectorCallback();
    VectorCallback* vectorCb2 = new VectorCallback();
    SimpleCallback* simpleCb1 = new SimpleCallback();
    /*
     * create listeners and closures that hold the name of the corresponding listener
     */
    Closure* closure1 = new Closure("Vectored Listener 1");
    TibrvVectorListener* listener1 = new TibrvVectorListener();
    Closure* closure2 = new Closure("Vectored Listener 2");
    TibrvVectorListener* listener2 = new TibrvVectorListener();
    Closure* closure3 = new Closure("Vectored Listener 3");
    TibrvVectorListener* listener3 = new TibrvVectorListener();
    Closure* closure4 = new Closure("Simple   Listener 4");
    TibrvListener* listener4 = new TibrvListener();
    // parse arguments for possible optional
    // parameters. These must precede the subject
    // and message strings.
    int i = getTransportParams(argc,argv);

    // open Tibrv
    status = Tibrv::open();
    if (status != TIBRV_OK)
    {
        fprintf(stderr,"Error: could not open TIB/RV, status=%d, text=%s\n",
            (int)status,status.getText());

        exit(-1);
    }

    // Create network transport
    TibrvNetTransport transport;
    status = transport.create(serviceStr,networkStr,daemonStr);
    if (status != TIBRV_OK)
    {
        fprintf(stderr,"Error: could not create transport, status=%d, text=%s\n",
            (int)status,status.getText());

        Tibrv::close();
        exit(-1);
    }
    transport.setDescription(argv[0]);

    // Create listeners for specified subjects.

    status=listener1->create(Tibrv::defaultQueue(),vectorCb1,&transport,"a.>",closure1);
    if (status != TIBRV_OK)
    {
        fprintf(stderr,"Error: could not create listener on %s, status=%d, text=%s\n",
                        argv[i],(int)status,status.getText());

        Tibrv::close();
        exit(-1);
    }

/******************************************************************************
******************************************************************************
*         IMPORTANT
* PASS  THE SAME VectorCallback INSTANCE TO listener1 AND listener2
* IF YOU WANT TO RECIEVE MESSAGES WITH SUBJECT a.> and b.>
* GROUPED TOGETHER IN THE ARRAY  "TibrvMsg* messages[]"
* void onMsgs(TibrvMsg* messages[], tibrv_u32 numMessages)
******************************************************************************
*****************************************************************************/
    status=listener2->create(Tibrv::defaultQueue(),vectorCb1,&transport,"b.>",closure2);
    if (status != TIBRV_OK)
    {
        fprintf(stderr,"Error: could not create listener on %s, status=%d, text=%s\n",
                        argv[i],(int)status,status.getText());

        Tibrv::close();
        exit(-1);
    }

    status=listener3->create(Tibrv::defaultQueue(),vectorCb2,&transport,"c.>",closure3);
    if (status != TIBRV_OK)
    {
        fprintf(stderr,"Error: could not create listener on %s, status=%d, text=%s\n",
                        argv[i],(int)status,status.getText());

        Tibrv::close();
        exit(-1);
    }

    status=listener4->create(Tibrv::defaultQueue(),simpleCb1,&transport,"a.1",closure4);
    if (status != TIBRV_OK)
    {
        fprintf(stderr,"Error: could not create listener on %s, status=%d, text=%s\n",
                        argv[i],(int)status,status.getText());

        Tibrv::close();
        exit(-1);
    }

    printf("Ready to receive message...\n");
    fflush(stdout);

    // dispatch Tibrv events

    do
    {
        status=Tibrv::defaultQueue()->dispatch();
    }while(status == TIBRV_OK );

    listener1->destroy();
    listener2->destroy();
    listener3->destroy();
    listener4->destroy();
    free(closure1);
    free(closure2);
    free(closure3);
    free(closure4);
    free(vectorCb1);
    free(vectorCb2);
    free(simpleCb1);
    Tibrv::close();
    printf("Exit gracefully...");
    exit(-1);

    return 0;  // to keep compiler happy

}
