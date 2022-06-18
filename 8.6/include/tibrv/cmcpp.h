/*
 * Copyright (c) 1995-$Date: 2016-12-13 12:21:58 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
 * All Rights Reserved. Confidential & Proprietary.
 * TIB/Rendezvous is protected under US Patent No. 5,187,787.
 * For more information, please contact:
 * TIBCO Software, Inc., Palo Alto, California, USA
 *
 */

#ifndef _INCLUDED_tibrvcmcpp_h
#define _INCLUDED_tibrvcmcpp_h

#include "tibrvcpp.h"
#include "cm.h"

//=====================================================================
// Forward declarations
//=====================================================================

class TibrvCmReviewCallback;
class TibrvCmTransport;
class TibrvCmTransportOnComplete;
class TibrvCmMsgCallback;
class TibrvCmListener;
class TibrvCmMsg;

class TibrvCmQueueTransport;
class TibrvCmQueueTransportOnComplete;

//=====================================================================
// TibrvCmReviewCallback
//=====================================================================

class TibrvCmReviewCallback
{

public:
    TibrvCmReviewCallback(){}
    virtual ~TibrvCmReviewCallback(){}

    virtual void* onLedgerMsg(TibrvCmTransport* cmTransport,
                              const char* subject,
                              TibrvMsg& msg,
                              void* closure) = 0;

};

//=====================================================================
// TibrvCmTransport
//=====================================================================

extern "C" void* TibrvCmTransport_CmReviewCB(tibrvcmTransport cmtransport,
                                             const char* subject,
                                             tibrvMsg msg,
                                             void* closure);
extern "C" void TibrvCmTransport_CompleteCB(tibrvcmTransport destroyedTransport,
                                            void* closure);

class TibrvCmTransport : public TibrvTransport {

    friend void* TibrvCmTransport_CmReviewCB(tibrvcmTransport cmtransport,
                                             const char* subject,
                                             tibrvMsg msg,
                                             void* closure);
    friend void TibrvCmTransport_CompleteCB(tibrvcmTransport destroyedTransport,
                                            void* closure);

public:
             TibrvCmTransport();
    virtual ~TibrvCmTransport();

    TibrvStatus create (TibrvTransport* transport);

    TibrvStatus create (TibrvTransport* transport,
                        const char* cmName,
                        tibrv_bool  requestOld,
                        const char* ledgerName = NULL,
                        tibrv_bool  syncLedger = TIBRV_FALSE,
                        const char* relayAgent = NULL);

    tibrv_bool  isValid () const;
    TibrvStatus destroy ();
    TibrvStatus destroyEx (TibrvCmTransportOnComplete* completeCB = NULL);

    TibrvStatus send        (const TibrvMsg& msg);
    TibrvStatus sendRequest (const TibrvMsg& msg,
                             TibrvMsg& replyMsg,
                             tibrv_f64 timeout);
    TibrvStatus sendReply   (const TibrvMsg& msg, const TibrvMsg& requestMsg);

    TibrvStatus createInbox (char* subjectString, tibrv_u32 subjectLimit) const;

    TibrvTransport* getTransport() const;

    TibrvStatus getName         (const char*& cmName) const;
    TibrvStatus getRequestOld   (tibrv_bool& requestOld) const;
    TibrvStatus getLedgerName   (const char*& ledgerName) const;
    TibrvStatus getSyncLedger   (tibrv_bool& syncLedgerL) const;
    TibrvStatus getRelayAgent   (const char*& relayAgent) const;

    TibrvStatus allowListener   (const char* cmName);
    TibrvStatus disallowListener(const char* cmName);

    TibrvStatus addListener     (const char* cmName, const char* subject);
    TibrvStatus removeListener  (const char* cmName, const char* subject);

    TibrvStatus removeSendState (const char* subject);

    TibrvStatus syncLedger();

    TibrvStatus connectToRelayAgent();
    TibrvStatus disconnectFromRelayAgent();

    TibrvStatus setDefaultTimeLimit(tibrv_f64 timeLimit);
    TibrvStatus getDefaultTimeLimit(tibrv_f64& timeLimit) const;

    TibrvStatus reviewLedger(TibrvCmReviewCallback* reviewCallback,
                             const char* subject,
                             const void* closure=NULL);

    TibrvStatus expireMessages(const char* subject,
                               tibrv_u64   sequenceNumber);

    TibrvStatus setPublisherInactivityDiscardInterval(tibrv_i32 timeout);

    tibrvcmTransport getHandle() const { return _cmTransport; }

protected:

    tibrvcmTransport _cmTransport;
    TibrvTransport*  _transport;
    TibrvCmTransportOnComplete* _completeCallback;

private:

    TibrvCmTransport(const TibrvCmTransport& cmTransport);
    TibrvCmTransport& operator=(const TibrvCmTransport& cmTransport);


};

//=====================================================================
// TibrvCmTransportOnComplete
//=====================================================================

class TibrvCmTransportOnComplete
{

 public:
    TibrvCmTransportOnComplete(){};
    virtual ~TibrvCmTransportOnComplete(){};

    virtual void onComplete(TibrvCmTransport* destroyedTransport) = 0;

};

//=====================================================================
// TibrvCmMsgCallback
//=====================================================================

class TibrvCmMsgCallback : public TibrvCallback
{

public:
    TibrvCmMsgCallback();

    virtual void onCmMsg(TibrvCmListener* cmListener, TibrvMsg& msg) = 0;

private:
    void onEvent(TibrvEvent * event, TibrvMsg& msg);

};

//=====================================================================
// TibrvCmListener
//=====================================================================

extern "C" void TibrvCmListener_CmListenCB (tibrvcmEvent event, tibrvMsg msg, 
                                            void *closure);
extern "C" void TibrvCmListener_CmCompleteCB(tibrvcmEvent event, void *closure);

class TibrvCmListener : public TibrvEvent {

    friend void TibrvCmListener_CmListenCB (tibrvcmEvent event, tibrvMsg msg, 
                                            void *closure);
    friend void TibrvCmListener_CmCompleteCB(tibrvcmEvent event, void *closure);

public:
             TibrvCmListener();
    virtual ~TibrvCmListener();

    TibrvStatus create(TibrvQueue* queue,
                       TibrvCmMsgCallback* cmMsgCallback,
                       TibrvCmTransport* cmTransport,
                       const char* subject,
                       const void* closure = NULL);

    tibrv_bool  isValid() const;
    TibrvStatus destroy(tibrv_bool cancelAgreements,
                        TibrvEventOnComplete* completeCB = NULL);

    TibrvCmTransport*   getTransport() const;

    TibrvStatus getSubject(const char *& subject) const;

    TibrvStatus setExplicitConfirm();
    TibrvStatus confirmMsg(const TibrvMsg& msg);

private:

    TibrvCmTransport*     _cmTransport;
    TibrvCmMsgCallback*   _cmCallback;
    tibrvcmEvent          _cmEvent;

    TibrvStatus destroy(TibrvEventOnComplete* completeCB = NULL)
                {USE_PARAM(completeCB);return TIBRV_NOT_PERMITTED;}

    TibrvCmListener(const TibrvCmListener& cmListener);
    TibrvCmListener& operator=(const TibrvCmListener& cmListener);

};

//=====================================================================
// TibrvCmMsg
//=====================================================================

class TibrvCmMsg
{

public:

    static TibrvStatus getSender    (TibrvMsg& msg, const char*& publisherName);
    static TibrvStatus getSequence  (TibrvMsg& msg, tibrv_u64& sequenceNumber);

    static TibrvStatus getTimeLimit (TibrvMsg& msg, tibrv_f64& timeToLive);
    static TibrvStatus setTimeLimit (TibrvMsg& msg, tibrv_f64  timeToLive);

private:

    TibrvCmMsg(){}

};


//=====================================================================
// TibrvCmQueueTransportOnComplete
//=====================================================================

class TibrvCmQueueTransportOnComplete
{

 public:
    TibrvCmQueueTransportOnComplete(){};
    virtual ~TibrvCmQueueTransportOnComplete(){};

    virtual void onComplete(TibrvCmQueueTransport* destroyedTransport) = 0;

};

//=====================================================================
// TibrvCmQueueTransport
//=====================================================================

extern "C" void TibrvCmQueueTransport_CompleteCB(tibrvcmTransport destroyedTransport,
                                                 void* closure);

class TibrvCmQueueTransport : public TibrvCmTransport
{

    friend void TibrvCmQueueTransport_CompleteCB(tibrvcmTransport destroyedTransport,
                                                 void* closure);

public:
             TibrvCmQueueTransport();
    virtual ~TibrvCmQueueTransport();

    TibrvStatus create
                (TibrvTransport* transport,
                 const char* cmName,
                 tibrv_u32 workerWeight = TIBRVCM_DEFAULT_WORKER_WEIGHT,
                 tibrv_u32 workerTasks  = TIBRVCM_DEFAULT_WORKER_TASKS,
                 tibrv_u16 schedulerWeight = TIBRVCM_DEFAULT_SCHEDULER_WEIGHT,
                 tibrv_f64 schedulerHeartbeat = TIBRVCM_DEFAULT_SCHEDULER_HB,
                 tibrv_f64 schedulerActivation = TIBRVCM_DEFAULT_SCHEDULER_ACTIVE
                );


    tibrv_bool      isValid () const;
    TibrvStatus     destroy ();
    TibrvStatus     destroyEx (TibrvCmQueueTransportOnComplete* completeCB = NULL);
    TibrvTransport* getTransport() const;

    TibrvStatus setCompleteTime(tibrv_f64  completeTime);
    TibrvStatus getCompleteTime(tibrv_f64& completeTime) const;

    TibrvStatus setWorkerWeight(tibrv_u32  workerWeight);
    TibrvStatus getWorkerWeight(tibrv_u32& workerWeight) const;

    TibrvStatus setWorkerTasks(tibrv_u32  workerTasks);
    TibrvStatus getWorkerTasks(tibrv_u32& workerTasks) const;

    TibrvStatus setTaskBacklogLimitInBytes(tibrv_u32 byteLimit);
    TibrvStatus setTaskBacklogLimitInMessages(tibrv_u32 messageLimit);

    TibrvStatus getUnassignedMessageCount(tibrv_u32& msgCount) const;

    tibrvcmTransport getHandle() const { return _cmTransport; }

protected:
    TibrvCmQueueTransportOnComplete*    _completeCallback;

private:

    TibrvCmQueueTransport(const TibrvCmQueueTransport& cmqTransport);
    TibrvCmQueueTransport& operator=(const TibrvCmQueueTransport& cmqTransport);

};

//=====================================================================
// inline implementations
//=====================================================================

_TIBRV_INLINE TibrvCmTransport* TibrvCmListener::getTransport() const { return _cmTransport; }
_TIBRV_INLINE TibrvTransport*   TibrvCmTransport::getTransport() const { return _transport; }
_TIBRV_INLINE TibrvTransport*   TibrvCmQueueTransport::getTransport() const { return _transport; }

//=====================================================================

#endif  /* _INCLUDED_tibrvcmcpp_h */
