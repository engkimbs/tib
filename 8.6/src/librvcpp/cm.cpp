/*
 * Copyright (c) 1995-$Date: 2016-12-13 12:21:58 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
 * All Rights Reserved. Confidential & Proprietary.
 * TIB/Rendezvous is protected under US Patent No. 5,187,787.
 * For more information, please contact:
 * TIBCO Software, Inc., Palo Alto, California, USA
 *
 */

#include "cmcpp.h"

//=====================================================================
// TibrvCmTransport
//=====================================================================

TibrvCmTransport::TibrvCmTransport() :
        TibrvTransport()
{
    _cmTransport = TIBRV_INVALID_ID;
    _transport   = NULL;
}

TibrvCmTransport::~TibrvCmTransport()
{
    destroy();
}


TibrvStatus TibrvCmTransport::create (TibrvTransport* transport)
{
    return create(transport,NULL,TIBRV_FALSE,NULL,TIBRV_FALSE,NULL);
}

TibrvStatus TibrvCmTransport::create (TibrvTransport* transport,
                    const char* cmName,
                    tibrv_bool  requestOld,
                    const char* ledgerName,
                    tibrv_bool  syncLedgerL,
                    const char* relayAgent)
{
    tibrv_status status;

    if (_cmTransport != TIBRV_INVALID_ID)
        return TIBRV_NOT_PERMITTED;

    if (transport == NULL)
        return TIBRV_INVALID_ARG;

    status = tibrvcmTransport_Create(&_cmTransport,
                                     transport->getHandle(),
                                     cmName,
                                     requestOld,
                                     ledgerName,
                                     syncLedgerL,
                                     relayAgent
                                    );
    if (status != TIBRV_OK)
    {
        _cmTransport = TIBRV_INVALID_ID;
        _transport = NULL;
    }
    else
    {
        _transport = transport;
    }

    return status;
}

tibrv_bool
TibrvCmTransport::isValid () const
{
    return ((_cmTransport != TIBRV_INVALID_ID) ? TIBRV_TRUE : TIBRV_FALSE);
}

void
TibrvCmTransport_CompleteCB(tibrvcmTransport destroyedTransport,
                            void* closure)
{
    USE_PARAM(destroyedTransport);

    if( !closure )
	return;

    TibrvCmTransport *t = static_cast<TibrvCmTransport *>(closure);
    TibrvCmTransportOnComplete *cb = t->_completeCallback;

    if( cb )
	cb->onComplete(t);
}

TibrvStatus
TibrvCmTransport::destroy()
{
    tibrv_status status = TIBRV_INVALID_TRANSPORT;
    if (_cmTransport != TIBRV_INVALID_ID)
    {
	status = tibrvcmTransport_Destroy(_cmTransport);
        _cmTransport = TIBRV_INVALID_ID;
        _transport = NULL;
    }
    return status;
}

TibrvStatus
TibrvCmTransport::destroyEx(TibrvCmTransportOnComplete* completeCB)
{
    tibrv_status status = TIBRV_INVALID_TRANSPORT;
    if (_cmTransport != TIBRV_INVALID_ID)
    {
	if( completeCB )
	{
	    _completeCallback = completeCB;
	    status = tibrvcmTransport_DestroyEx(_cmTransport,
                                                TibrvCmTransport_CompleteCB,
                                                this);
	}
	else
	{
	    status = tibrvcmTransport_Destroy(_cmTransport);
	}
        _cmTransport = TIBRV_INVALID_ID;
        _transport = NULL;
    }
    return status;
}

TibrvStatus
TibrvCmTransport::send(const TibrvMsg& msg)
{
    return tibrvcmTransport_Send(_cmTransport,msg.getHandle());
}

TibrvStatus
TibrvCmTransport::sendRequest(const TibrvMsg& sendMsg, TibrvMsg& replyMsg,
                              tibrv_f64 timeout)
{
    tibrvMsg msg;
    TibrvStatus status = tibrvcmTransport_SendRequest(_cmTransport,sendMsg.getHandle(),&msg,timeout);
    if (status == TIBRV_OK)
        replyMsg.attach(msg,TIBRV_TRUE);

    return status;
}

TibrvStatus
TibrvCmTransport::sendReply(const TibrvMsg& msg, const TibrvMsg& requestMsg)
{
    return tibrvcmTransport_SendReply(_cmTransport,msg.getHandle(),requestMsg.getHandle());
}

TibrvStatus
TibrvCmTransport::createInbox(char* subjectString, tibrv_u32 subjectLimit) const
{
    if (_transport != NULL)
        return _transport->createInbox(subjectString,subjectLimit);
    return TIBRV_INVALID_TRANSPORT;
}

TibrvStatus
TibrvCmTransport::getName(const char*& cmName) const
{
    return tibrvcmTransport_GetName(_cmTransport,&cmName);
}

TibrvStatus
TibrvCmTransport::getRequestOld(tibrv_bool& requestOld) const
{
    return tibrvcmTransport_GetRequestOld(_cmTransport,&requestOld);
}

TibrvStatus
TibrvCmTransport::getLedgerName(const char*& ledgerName) const
{
    return tibrvcmTransport_GetLedgerName(_cmTransport,&ledgerName);
}

TibrvStatus
TibrvCmTransport::getSyncLedger(tibrv_bool& syncLedgerL) const
{
    return tibrvcmTransport_GetSyncLedger(_cmTransport,&syncLedgerL);
}

TibrvStatus
TibrvCmTransport::getRelayAgent(const char*& relayAgent) const
{
    return tibrvcmTransport_GetRelayAgent(_cmTransport,&relayAgent);
}

TibrvStatus
TibrvCmTransport::allowListener(const char* cmName)
{
    return tibrvcmTransport_AllowListener(_cmTransport,cmName);
}

TibrvStatus
TibrvCmTransport::disallowListener(const char* cmName)
{
    return tibrvcmTransport_DisallowListener(_cmTransport,cmName);
}

TibrvStatus
TibrvCmTransport::addListener(const char* cmName, const char* subject)
{
    return tibrvcmTransport_AddListener(_cmTransport,cmName,subject);
}

TibrvStatus
TibrvCmTransport::removeListener(const char* cmName, const char* subject)
{
    return tibrvcmTransport_RemoveListener(_cmTransport,cmName,subject);
}

TibrvStatus
TibrvCmTransport::removeSendState(const char* subject)
{
    return tibrvcmTransport_RemoveSendState(_cmTransport,subject);
}

TibrvStatus
TibrvCmTransport::syncLedger()
{
    return tibrvcmTransport_SyncLedger(_cmTransport);
}

TibrvStatus
TibrvCmTransport::connectToRelayAgent()
{
    return tibrvcmTransport_ConnectToRelayAgent(_cmTransport);
}

TibrvStatus
TibrvCmTransport::disconnectFromRelayAgent()
{
    return tibrvcmTransport_DisconnectFromRelayAgent(_cmTransport);
}

TibrvStatus
TibrvCmTransport::setDefaultTimeLimit(tibrv_f64 timeLimit)
{
    return tibrvcmTransport_SetDefaultCMTimeLimit(_cmTransport,timeLimit);
}

TibrvStatus
TibrvCmTransport::getDefaultTimeLimit(tibrv_f64& timeLimit) const
{
    return tibrvcmTransport_GetDefaultCMTimeLimit(_cmTransport,&timeLimit);
}

TibrvStatus
TibrvCmTransport::expireMessages(const char* subject,
				 tibrv_u64   sequenceNumber)
{
    return tibrvcmTransport_ExpireMessages(_cmTransport, subject, sequenceNumber);
}

TibrvStatus
TibrvCmTransport::setPublisherInactivityDiscardInterval(tibrv_i32 timeout)
{
    return tibrvcmTransport_SetPublisherInactivityDiscardInterval(_cmTransport, timeout);
}

typedef struct
{
    TibrvCmTransport* cmTport;
    TibrvCmReviewCallback* cb;
    void *closure;

} _cmReviewData;

void*
TibrvCmTransport_CmReviewCB(tibrvcmTransport cmtransport,
                            const char* subject,
                            tibrvMsg msg,
                            void* closure)
{
    USE_PARAM(cmtransport);

    if (closure == NULL)
        return (reinterpret_cast<void*>(1));   // stop

    _cmReviewData* reviewData = static_cast<_cmReviewData*>(closure);

    TibrvMsg message(msg,TIBRV_FALSE);
    return
        reviewData->cb->onLedgerMsg(reviewData->cmTport,
                                    subject,
                                    message,
                                    reviewData->closure);

}

TibrvStatus
TibrvCmTransport::reviewLedger(TibrvCmReviewCallback* reviewCallback,
                               const char* subject,
                               const void* closure)
{
    if (reviewCallback == NULL)
        return TIBRV_INVALID_ARG;

    if (subject == NULL)
        return TIBRV_INVALID_ARG;

    _cmReviewData reviewData;
    reviewData.cmTport = this;
    reviewData.cb      = reviewCallback;
    reviewData.closure = const_cast<void*>(closure);

    return tibrvcmTransport_ReviewLedger(_cmTransport,
                                         TibrvCmTransport_CmReviewCB,
                                         subject,
                                         &reviewData);
}

TibrvCmTransport::TibrvCmTransport(const TibrvCmTransport& cmtport) :
        TibrvTransport()
{
    _cmTransport = cmtport.getHandle();
}

TibrvCmTransport& TibrvCmTransport::operator=(const TibrvCmTransport& cmtport)
{
    _cmTransport = cmtport.getHandle();
    return *this;
}

//=====================================================================
// TibrvCmMsgCallback
//=====================================================================

TibrvCmMsgCallback::TibrvCmMsgCallback() :
        TibrvCallback()
{
}

void
TibrvCmMsgCallback::onEvent(TibrvEvent *, TibrvMsg& msg)
{
    // this method is never used, onCmMsg is called instead.
    if (msg.getHandle() == NULL)
        msg.reset();
}


//=====================================================================
// TibrvCmListener
//=====================================================================

void TibrvCmListener_CmCompleteCB(tibrvcmEvent event, void *closure)
{
    USE_PARAM(event);

    if (closure == NULL)
        return;

    TibrvCmListener * pEvt = static_cast<TibrvCmListener*>(closure);
    TibrvEventOnComplete * cb = pEvt->_completeCallback;
    if (cb != NULL)
        cb->onComplete(pEvt);
}


void TibrvCmListener_CmListenCB(tibrvcmEvent cmEvent, tibrvMsg msg, void *closure)
{
    USE_PARAM(cmEvent);

    if (closure == NULL)
        return;

    TibrvCmListener * cmListener = static_cast<TibrvCmListener*>(closure);
    TibrvCmMsgCallback * cb = cmListener->_cmCallback;
    if (cb == NULL)
        return;

    TibrvMsg message(msg,TIBRV_FALSE);
    cb->onCmMsg(cmListener,message);
}


TibrvCmListener::TibrvCmListener() :
    TibrvEvent()
{
    _cmEvent = TIBRV_INVALID_ID;
    _cmTransport = NULL;
    _cmCallback = NULL;
    _completeCallback = NULL;
    _objType = TIBRV_LISTEN_EVENT;
}

TibrvCmListener::~TibrvCmListener()
{
    destroy(TIBRV_FALSE);
}

TibrvStatus
TibrvCmListener::create(TibrvQueue* queue,
                       TibrvCmMsgCallback* cmMsgCallback,
                       TibrvCmTransport* cmTransport,
                       const char* subject,
                       const void* closure)
{
    tibrv_status status;
    tibrvcmTransport tport;

    if (cmTransport == NULL)
        return TIBRV_INVALID_ARG;

    if (cmMsgCallback == NULL)
        return TIBRV_INVALID_ARG;

    if (subject == NULL)
        return TIBRV_INVALID_ARG;

    if (_cmEvent != TIBRV_INVALID_ID)
        return TIBRV_NOT_PERMITTED;

    tibrvQueue q = TIBRV_DEFAULT_QUEUE;
    if (queue != NULL)
    {
        _queue = queue;
        q = _queue->getHandle();
    }
    else
        return TIBRV_INVALID_QUEUE;

    _cmTransport = cmTransport;
    tport = _cmTransport->getHandle();

    _cmCallback = cmMsgCallback;
    _closure = const_cast<void*>(closure);

    status = tibrvcmEvent_CreateListener(&_cmEvent,q,
                                         TibrvCmListener_CmListenCB,
                                         tport,subject,this);
    if (status != TIBRV_OK)
        _cmEvent = TIBRV_INVALID_ID;

    return status;
}

tibrv_bool
TibrvCmListener::isValid() const
{
    return ((_cmEvent != TIBRV_INVALID_ID) ? TIBRV_TRUE : TIBRV_FALSE);
}

TibrvStatus
TibrvCmListener::destroy(tibrv_bool cancelCmAgreements,
                        TibrvEventOnComplete* completeCallback)
{
    tibrv_status status = TIBRV_INVALID_EVENT;

    if (_cmEvent != TIBRV_INVALID_ID)
    {
        if (completeCallback == NULL)
        {
            status = tibrvcmEvent_Destroy(_cmEvent,cancelCmAgreements);
        }
        else
        {
            _completeCallback = completeCallback;
            status = tibrvcmEvent_DestroyEx(_cmEvent,cancelCmAgreements,
                                            TibrvEvent_CompleteCB);
        }
        _cmEvent = TIBRV_INVALID_ID;
    }
    return status;
}

TibrvStatus
TibrvCmListener::getSubject(const char *& subject) const
{
    return tibrvcmEvent_GetListenerSubject(_cmEvent,&subject);
}

TibrvStatus
TibrvCmListener::setExplicitConfirm()
{
    return tibrvcmEvent_SetExplicitConfirm(_cmEvent);
}

TibrvStatus
TibrvCmListener::confirmMsg(const TibrvMsg& msg)
{
    return tibrvcmEvent_ConfirmMsg(_cmEvent,msg.getHandle());
}

TibrvCmListener::TibrvCmListener(const TibrvCmListener& cmlistener)  :
    TibrvEvent()
{
    _event = cmlistener.getHandle();
}

TibrvCmListener& TibrvCmListener::operator=(const TibrvCmListener& cmlistener)
{
    _event = cmlistener.getHandle();
    return *this;
}

//=====================================================================
// TibrvCmMsg
//=====================================================================

TibrvStatus
TibrvCmMsg::getSender(TibrvMsg& msg, const char*& publisherName)
{
    return tibrvMsg_GetCMSender(msg.getHandle(),&publisherName);
}

TibrvStatus
TibrvCmMsg::getSequence(TibrvMsg& msg, tibrv_u64& sequenceNumber)
{
    return tibrvMsg_GetCMSequence(msg.getHandle(),&sequenceNumber);
}

TibrvStatus
TibrvCmMsg::getTimeLimit(TibrvMsg& msg, tibrv_f64& timeLimit)
{
    return tibrvMsg_GetCMTimeLimit(msg.getHandle(),&timeLimit);
}

TibrvStatus
TibrvCmMsg::setTimeLimit(TibrvMsg& msg, tibrv_f64 timeLimit)
{
    return tibrvMsg_SetCMTimeLimit(msg.getHandle(),timeLimit);
}

//=====================================================================

