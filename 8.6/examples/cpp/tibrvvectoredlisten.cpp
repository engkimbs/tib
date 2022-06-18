/*
 * Copyright (c) 1998-$Date: 2016-12-13 12:47:31 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
 * All Rights Reserved. Confidential & Proprietary.
 * TIB/Rendezvous is protected under US Patent No. 5,187,787.
 * For more information, please contact:
 * TIBCO Software Inc., Palo Alto, California, USA
 *
 */

/*
 * tibrvvectorlisten - generic TIB/Rendezvous subscriber
 *
 * This program listens for any number of messages on a specified
 * set of subject(s).  Message(s) received are printed.
 *
 * Some platforms require proper quoting of the arguments to prevent
 * the command line processor from modifying the command arguments.
 *
 * The user may terminate the program by typing Control-C.
 *
 * Optionally the user may specify communication parameters for
 * tibrvTransport_Create.  If none are specified, default values
 * are used.  For information on default values for these parameters,
 * please see the TIBCO/Rendezvous Concepts manual.
 *
 *
 * Examples:
 *
 * Listen to every message published on subject a.b.c:
 *  tibrvvectorlisten a.b.c
 *
 * Listen to every message published on subjects a.b.c and x.*.Z:
 *  tibrvvectorlisten a.b.c "x.*.Z"
 *
 * Listen to messages published on subject a.b.c using port 7566:
 *  tibrvvectorlisten -service 7566 a.b.c
 *
 */

#include <stdlib.h>
#include <stdio.h>
#include <signal.h>
#include <string.h>
#include <time.h>

#include "tibrv/tibrvcpp.h"

#define MIN_PARAMS  (2)

/*********************************************************************/
/* Transport parameters                                              */
/*********************************************************************/

char* serviceStr = NULL;
char* networkStr = NULL;
char* daemonStr  = NULL;

/*********************************************************************/
/* usage:                                                            */
/*         print usage information and quit                          */
/*********************************************************************/
void usage()
{
    fprintf(stderr,"tibrvvectorlisten [-service service] [-network network] \n");
    fprintf(stderr,"            [-daemon daemon] subject_list\n");
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
class MsgsCallback : public TibrvVectorCallback
{
public:

    void onMsgs(TibrvMsg* msgs[], tibrv_u32 nrMessages)
    {
    	tibrv_u32	i			 = 0;
        const char* sendSubject  = NULL;
        const char* replySubject = NULL;
        const char* msgString    = NULL;

        for(i=0;i<nrMessages;i++)
        {
            // Get the subject name to which this message was sent
            msgs[i]->getSendSubject(sendSubject);

            // If there was a reply subject, get it
            msgs[i]->getReplySubject(replySubject);

            // Convert the incoming message to a string
            msgs[i]->convertToString(msgString);

            if (replySubject != NULL)
            printf("subject=%s, reply=%s, message=%s\n",sendSubject, replySubject, msgString);
            else
                printf("subject=%s, message=%s\n",sendSubject, msgString);

            fflush(stdout);
        }
    }

};

/*********************************************************************/
/* main                                                              */
/*********************************************************************/
int main(int argc, char** argv)
{
    TibrvStatus status;

    if (argc < MIN_PARAMS)
        usage();

    // parse arguments for possible optional
    // parameters. These must precede the subject
    // and message strings.
    int i = getTransportParams(argc,argv);

    // we must have at least one subject
    if (i >= argc)
        usage();

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
    // In this test program we never delete listener objects.
    while (i < argc)
    {
    	TibrvVectorListener* listener = new TibrvVectorListener();
        status=listener->create(Tibrv::defaultQueue(),new MsgsCallback(),&transport,argv[i]);
        if (status != TIBRV_OK)
        {
            fprintf(stderr,"Error: could not create listener on %s, status=%d, text=%s\n",
                argv[i],(int)status,status.getText());
            Tibrv::close();
            exit(-1);
        }
        printf("Listening on: %s\n",argv[i]);
        fflush(stdout);
        i++;
    }

    // dispatch Tibrv events
    while((status=Tibrv::defaultQueue()->dispatch()) == TIBRV_OK);

    fprintf(stderr,"Error: dispatch failed, status=%d, text=%s\n",
        (int)status,status.getText());
    Tibrv::close();
    exit(-1);

    return 0;  

}
