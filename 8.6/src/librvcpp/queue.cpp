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
// TibrvQueue
//=====================================================================

void TibrvQueue_CompleteCB(tibrvQueue queue, void *closure)
{
    USE_PARAM(queue);

    if (closure == NULL)
        return;

    TibrvQueue * pQ = static_cast<TibrvQueue*>(closure);
    TibrvQueueOnComplete * cb = pQ->_completeCallback;
    if (cb != NULL)
        cb->onComplete(pQ,pQ->_closure);
}

TibrvQueue::TibrvQueue()
{
    _queue = TIBRV_INVALID_ID;
    _closure = NULL;
}

// private
TibrvQueue::TibrvQueue(tibrvQueue queue)
{
    _queue = queue;
}

TibrvQueue::~TibrvQueue()
{
    // ignoring the error
    destroy();
}


TibrvStatus
TibrvQueue::create()
{
    if (_queue != TIBRV_INVALID_ID) return TIBRV_NOT_PERMITTED;
    if (_queue == TIBRV_DEFAULT_QUEUE) return TIBRV_OK;
    return tibrvQueue_Create(&_queue);
}

TibrvStatus
TibrvQueue::destroy()
{
    return destroy(NULL,NULL);
}

TibrvStatus
TibrvQueue::destroy(TibrvQueueOnComplete* completeCB, const void* closure)
{
    // do not destroy default queue
    if (_queue == TIBRV_DEFAULT_QUEUE)
        return TIBRV_NOT_PERMITTED;

    tibrv_status status = TIBRV_OK;

    if (_queue != TIBRV_INVALID_ID)
    {
        if (completeCB == NULL)
        {
            status = tibrvQueue_Destroy(_queue);
        }
        else
        {
            _completeCallback = completeCB;
            _closure = const_cast<void*>(closure);
            status = tibrvQueue_DestroyEx(_queue, TibrvQueue_CompleteCB,this);
        }
        _queue = TIBRV_INVALID_ID;
    }
    return status;
}

tibrv_bool
TibrvQueue::isValid() const
{
    return ((_queue != TIBRV_INVALID_ID) ? TIBRV_TRUE : TIBRV_FALSE);
}

TibrvStatus
TibrvQueue::getCount(tibrv_u32& numEvents) const
{
    return tibrvQueue_GetCount(_queue,&numEvents);
}

TibrvStatus
TibrvQueue::setName(const char* queueName)
{
    return tibrvQueue_SetName(_queue,queueName);
}

TibrvStatus
TibrvQueue::getName(const char*& queueName) const
{
    return tibrvQueue_GetName(_queue,&queueName);
}

TibrvStatus
TibrvQueue::dispatch()
{
    return tibrvQueue_TimedDispatch(_queue,TIBRV_WAIT_FOREVER);
}

TibrvStatus
TibrvQueue::timedDispatch(tibrv_f64 timeout)
{
    return tibrvQueue_TimedDispatch(_queue,timeout);
}

TibrvStatus
TibrvQueue::poll()
{
    return tibrvQueue_TimedDispatch(_queue,TIBRV_NO_WAIT);
}

TibrvStatus
TibrvQueue::setPriority(tibrv_u32 priority)
{
    return tibrvQueue_SetPriority(_queue,priority);
}

TibrvStatus
TibrvQueue::getPriority(tibrv_u32& priority) const
{
    return tibrvQueue_GetPriority(_queue,&priority);
}

TibrvStatus
TibrvQueue::setLimitPolicy (tibrvQueueLimitPolicy policy,
                            tibrv_u32 maxEvents,
                            tibrv_u32 discardAmount)
{
    return tibrvQueue_SetLimitPolicy(_queue,policy,maxEvents,discardAmount);
}

TibrvStatus
TibrvQueue::getLimitPolicy (tibrvQueueLimitPolicy& policy,
                            tibrv_u32& maxEvents,
                            tibrv_u32& discardAmount) const
{
    return tibrvQueue_GetLimitPolicy(_queue,&policy,&maxEvents,&discardAmount);
}

TibrvQueue::TibrvQueue(const TibrvQueue& queue) : TibrvDispatchable()
{
    _queue = queue.getHandle();
}

TibrvQueue& TibrvQueue::operator=(const TibrvQueue& queue)
{
    _queue = queue.getHandle();
    return *this;
}
