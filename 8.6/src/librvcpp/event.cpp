/*
 * Copyright (c) 1995-$Date: 2016-12-13 12:21:58 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
 * All Rights Reserved. Confidential & Proprietary.
 * TIB/Rendezvous is protected under US Patent No. 5,187,787.
 * For more information, please contact:
 * TIBCO Software, Inc., Palo Alto, California, USA
 *
 */

#include "tibrvcpp.h"

//=====================================================================
// TibrvEvent
//=====================================================================

void TibrvEvent_CompleteCB(tibrvEvent event, void *closure)
{
    USE_PARAM(event);

    if (closure == NULL)
        return;

    TibrvEvent * pEvt = static_cast<TibrvEvent*>(closure);
    TibrvEventOnComplete * cb = pEvt->_completeCallback;
    if (cb != NULL)
        cb->onComplete(pEvt);
}

TibrvEvent::TibrvEvent()
{
    _event = TIBRV_INVALID_ID;
    _callback = NULL;
    _completeCallback = NULL;
    _closure = NULL;
    _queue = NULL;
    _objType = static_cast<tibrvEventType>(0);
}

TibrvEvent::~TibrvEvent()
{
    destroy();
}

TibrvStatus
TibrvEvent::destroy(TibrvEventOnComplete* completeCallback)
{
    tibrv_status status = TIBRV_INVALID_EVENT;
    if (_event != TIBRV_INVALID_ID)
    {
        if (completeCallback == NULL)
        {
            status = tibrvEvent_Destroy(_event);
        }
        else
        {
            _completeCallback = completeCallback;
            status = tibrvEvent_DestroyEx(_event,
                                          TibrvEvent_CompleteCB);
        }
        _event = TIBRV_INVALID_ID;
    }
    return status;
}

tibrv_bool
TibrvEvent::isValid() const
{
    return ((_event != TIBRV_INVALID_ID) ? TIBRV_TRUE : TIBRV_FALSE);
}


TibrvStatus
TibrvEvent::getType(tibrvEventType& type) const
{
    return tibrvEvent_GetType(_event,&type);
}

tibrv_bool
TibrvEvent::isListener() const
{
    return ((_objType == TIBRV_LISTEN_EVENT) ? TIBRV_TRUE : TIBRV_FALSE);
}

tibrv_bool
TibrvEvent::isVectorListener() const
{
    return (((_objType == TIBRV_LISTEN_EVENT) && (_vectorCallback != NULL)) ? TIBRV_TRUE : TIBRV_FALSE);
}
tibrv_bool
TibrvEvent::isTimer() const
{
    return ((_objType == TIBRV_TIMER_EVENT) ? TIBRV_TRUE : TIBRV_FALSE);
}

tibrv_bool
TibrvEvent::isIOEvent() const
{
    return ((_objType == TIBRV_IO_EVENT) ? TIBRV_TRUE : TIBRV_FALSE);
}

TibrvEvent::TibrvEvent(const TibrvEvent& event)
{
    _event = event.getHandle();
}

TibrvEvent& TibrvEvent::operator=(const TibrvEvent& event)
{
    _event = event.getHandle();
    return *this;
}

//=====================================================================
// TibrvListener
//=====================================================================

void TibrvEvent_ListenCB(tibrvEvent event, tibrvMsg msg, void *closure)
{
    USE_PARAM(event);

    if (closure == NULL)
        return;

    TibrvListener * listener = static_cast<TibrvListener*>(closure);
    TibrvCallback * cb = listener->_callback;
    if (cb == NULL)
        return;

    TibrvMsg rvmsg(msg,TIBRV_FALSE);
    rvmsg.setEvent(listener);
    cb->onEvent(listener,rvmsg);

}

TibrvListener::TibrvListener() :
    TibrvEvent()
{
    _transport = NULL;
    _objType = TIBRV_LISTEN_EVENT;
}

TibrvListener::~TibrvListener()
{
}

TibrvStatus
TibrvListener::create(TibrvQueue* queue,
                      TibrvCallback* callback,
                      TibrvTransport* transport,
                      const char* subject,
                      const void* closure)
{
    if (_event != TIBRV_INVALID_ID)
        return TIBRV_NOT_PERMITTED;

    if (callback == NULL)
        return TIBRV_INVALID_ARG;

    if (transport == NULL)
        return TIBRV_INVALID_ARG;

    if (subject == NULL)
        return TIBRV_INVALID_ARG;

    tibrv_status status;
    tibrvQueue q = TIBRV_DEFAULT_QUEUE;
    tibrvTransport t = TIBRV_PROCESS_TRANSPORT;
    if (queue != NULL)
    {
        _queue = queue;
        q = _queue->getHandle();
    }
    else
        return TIBRV_INVALID_QUEUE;

    _transport = transport;
    t = _transport->getHandle();

    _callback = callback;
    _closure = const_cast<void*>(closure);
    status = tibrvEvent_CreateListener(&_event,q,TibrvEvent_ListenCB,t,subject,this);

    if (status != TIBRV_OK)
        _event = TIBRV_INVALID_ID;

    return status;
}

TibrvTransport*
TibrvListener::getTransport() const
{
    return _transport;
}

TibrvStatus
TibrvListener::getSubject(const char*& subject) const
{
    return tibrvEvent_GetListenerSubject(_event,&subject);
}

TibrvListener::TibrvListener(const TibrvListener& event) :
    TibrvEvent(event)
{
    _event = event.getHandle();
}

TibrvListener& TibrvListener::operator=(const TibrvListener& event)
{
    _event = event.getHandle();
    return *this;
}

//=====================================================================
// TibrvVectorListener
//=====================================================================

void TibrvEvent_VectorListenCB ( tibrvMsg messages[], tibrv_u32 numMessages)
{
    void* closure  = NULL;
    tibrv_status   status = TIBRV_OK;
    tibrv_u32            i=0;
    TibrvMsg**      rvmsgs  =  new TibrvMsg*[numMessages];
    TibrvVectorListener* listener = NULL;
    TibrvVectorCallback* cb = NULL;
    if((0 == numMessages) || (NULL == rvmsgs))
        return;

    for(i=0;i<numMessages;i++)
    {
        rvmsgs[i] = new TibrvMsg(messages[i],TIBRV_FALSE);
        status = tibrvMsg_GetClosure(messages[i],&closure);

        if (TIBRV_OK != status || (closure == NULL))
            return;

        listener = static_cast<TibrvVectorListener*>(closure);
        if (listener == NULL)
            return;

        cb = listener->_vectorCallback;

        if (cb == NULL)
            return;

        if(rvmsgs[i]!= NULL)
            rvmsgs[i]->setEvent(listener);
        else
            return;
    }

    cb->onMsgs(rvmsgs,numMessages);

    for(i=0;i<numMessages;i++)
    {
        delete rvmsgs[i];
    }

    delete []rvmsgs;

}

TibrvVectorListener::TibrvVectorListener() :
    TibrvEvent()
{
    _transport = NULL;
    _objType = TIBRV_LISTEN_EVENT;
}

TibrvVectorListener::~TibrvVectorListener()
{
}

TibrvStatus
TibrvVectorListener::create(TibrvQueue* queue,
                      TibrvVectorCallback* callback,
                      TibrvTransport* transport,
                      const char* subject,
                      const void* closure)
{
    if (_event != TIBRV_INVALID_ID)
        return TIBRV_NOT_PERMITTED;

    if (callback == NULL)
        return TIBRV_INVALID_ARG;

    if (transport == NULL)
        return TIBRV_INVALID_ARG;

    if (subject == NULL)
        return TIBRV_INVALID_ARG;

    tibrv_status status;
    tibrvQueue q = TIBRV_DEFAULT_QUEUE;
    tibrvTransport t = TIBRV_PROCESS_TRANSPORT;
    if (queue != NULL)
    {
        _queue = queue;
        q = _queue->getHandle();
    }
    else
        return TIBRV_INVALID_QUEUE;

    _transport = transport;
    t = _transport->getHandle();

    _vectorCallback = callback;
    _closure = const_cast<void*>(closure);
    status = tibrvEvent_CreateGroupVectorListener(&_event,q,TibrvEvent_VectorListenCB,t,subject,this,callback);

    if (status != TIBRV_OK)
        _event = TIBRV_INVALID_ID;

    return status;
}

TibrvTransport*
TibrvVectorListener::getTransport() const
{
    return _transport;
}

TibrvStatus
TibrvVectorListener::getSubject(const char*& subject) const
{
    return tibrvEvent_GetListenerSubject(_event,&subject);
}

TibrvVectorListener::TibrvVectorListener(const TibrvVectorListener& event) :
    TibrvEvent(event)
{
    _event = event.getHandle();
}

TibrvVectorListener& TibrvVectorListener::operator=(const TibrvVectorListener& event)
{
    _event = event.getHandle();
    return *this;
}

//=====================================================================
// TibrvTimer
//=====================================================================

void TibrvEvent_TimerCB(tibrvEvent event, tibrvMsg msg, void *closure)
{
    USE_PARAM(event);
    USE_PARAM(msg);

    if (closure == NULL)
        return;

    TibrvTimer * timer = static_cast<TibrvTimer*>(closure);
    TibrvCallback * cb = timer->_callback;
    TibrvMsg rvmsg;
    if (cb == NULL)
        return;

    cb->onEvent(timer,rvmsg);

}

TibrvTimer::TibrvTimer() :
    TibrvEvent()
{
    //_interval = (tibrv_f64)0;
    _objType = TIBRV_TIMER_EVENT;
}

TibrvTimer::~TibrvTimer()
{
}

TibrvStatus
TibrvTimer::create(TibrvQueue* queue,
                   TibrvCallback* callback,
                   tibrv_f64 interval,
                   const void* closure)
{
    if (_event != TIBRV_INVALID_ID)
        return TIBRV_NOT_PERMITTED;

    if (callback == NULL)
        return TIBRV_INVALID_ARG;

    tibrv_status status;
    tibrvQueue q = TIBRV_DEFAULT_QUEUE;

    if (queue != NULL)
    {
        _queue = queue;
        q = _queue->getHandle();
    }
    else
        return TIBRV_INVALID_QUEUE;

    _callback = callback;
    _closure = const_cast<void*>(closure);
    status = tibrvEvent_CreateTimer(&_event,q,TibrvEvent_TimerCB,interval,this);
    if (status != TIBRV_OK)
        _event = TIBRV_INVALID_ID;
    return status;
}

TibrvStatus
TibrvTimer::getInterval(tibrv_f64& interval) const
{
    return tibrvEvent_GetTimerInterval(_event,&interval);
}

TibrvStatus
TibrvTimer::resetInterval(tibrv_f64 newInterval)
{
    return tibrvEvent_ResetTimerInterval(_event,newInterval);
}

TibrvTimer::TibrvTimer(const TibrvTimer& event) :
    TibrvEvent(event)
{
    _event = event.getHandle();
}

TibrvTimer& TibrvTimer::operator=(const TibrvTimer& event)
{
    _event = event.getHandle();
    return *this;
}

//=====================================================================
// TibrvIOEvent
//=====================================================================

void TibrvEvent_IoCB(tibrvEvent event, tibrvMsg msg, void *closure)
{
    USE_PARAM(event);
    USE_PARAM(msg);

    if (closure == NULL)
        return;

    TibrvIOEvent * ioEvent = static_cast<TibrvIOEvent*>(closure);
    TibrvCallback * cb = ioEvent->_callback;
    if (cb == NULL)
        return;

    TibrvMsg rvmsg;   // empty message
    cb->onEvent(ioEvent,rvmsg);
}


TibrvIOEvent::TibrvIOEvent() :
    TibrvEvent()
{
    _objType = TIBRV_IO_EVENT;
}

TibrvIOEvent::~TibrvIOEvent()
{
}

TibrvStatus
TibrvIOEvent::create(TibrvQueue* queue,
                     TibrvCallback* callback,
                     tibrv_i32 socketId,
                     tibrvIOType ioType,
                     const void* closure)
{
    if (_event != TIBRV_INVALID_ID)
        return TIBRV_NOT_PERMITTED;

    if (callback == NULL)
        return TIBRV_INVALID_ARG;

    tibrv_status status;
    tibrvQueue q = TIBRV_DEFAULT_QUEUE;

    if (queue != NULL)
    {
        _queue = queue;
        q = _queue->getHandle();
    }
    else
        return TIBRV_INVALID_QUEUE;

    _callback = callback;
    _closure = const_cast<void*>(closure);
    status = tibrvEvent_CreateIO(&_event,q,TibrvEvent_IoCB,socketId,ioType,this);
    if (status != TIBRV_OK)
        _event = TIBRV_INVALID_ID;

    return status;
}

TibrvStatus
TibrvIOEvent::getIOSource(tibrv_i32& ioSource) const
{
    return tibrvEvent_GetIOSource(_event,&ioSource);
}

TibrvStatus
TibrvIOEvent::getIOType(tibrvIOType& ioType) const
{
    return tibrvEvent_GetIOType(_event,&ioType);
}


TibrvIOEvent::TibrvIOEvent(const TibrvIOEvent& event) :
    TibrvEvent(event)
{
    _event = event.getHandle();
}

TibrvIOEvent& TibrvIOEvent::operator=(const TibrvIOEvent& event)
{
    _event = event.getHandle();
    return *this;
}

//=====================================================================
// TibrvMsgCallback
//=====================================================================

TibrvMsgCallback::TibrvMsgCallback() :
        TibrvCallback()
{
}

void
TibrvMsgCallback::onEvent(TibrvEvent * event, TibrvMsg& msg)
{
    onMsg(static_cast<TibrvListener*>(event), msg);
}

//=====================================================================
// TibrvTimerCallback
//=====================================================================

TibrvTimerCallback::TibrvTimerCallback() :
        TibrvCallback()
{
}

void
TibrvTimerCallback::onEvent(TibrvEvent * event, TibrvMsg& msg)
{
    if (msg.getHandle() == NULL)
        msg.detach();
    onTimer(static_cast<TibrvTimer*>(event));
}


//=====================================================================
// TibrvIOCallback
//=====================================================================

TibrvIOCallback::TibrvIOCallback() :
        TibrvCallback()
{
}

void
TibrvIOCallback::onEvent(TibrvEvent * event, TibrvMsg& msg)
{
    if (msg.getHandle() == NULL)
        msg.detach();
    onIOEvent(static_cast<TibrvIOEvent*>(event));
}

//=====================================================================
