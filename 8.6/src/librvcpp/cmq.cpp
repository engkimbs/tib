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
// TibrvCmQueueTransport
//=====================================================================

TibrvCmQueueTransport::TibrvCmQueueTransport() :
        TibrvCmTransport()
{
}

TibrvCmQueueTransport::~TibrvCmQueueTransport()
{
    destroy();
}


TibrvStatus TibrvCmQueueTransport::create
                (TibrvTransport* transport,
                 const char* cmName,
                 tibrv_u32 workerWeight,
                 tibrv_u32 workerTasks,
                 tibrv_u16 schedulerWeight,
                 tibrv_f64 schedulerHeartbeat,
                 tibrv_f64 schedulerActivation)
{
    tibrv_status status;

    if (_cmTransport != TIBRV_INVALID_ID)
        return TIBRV_NOT_PERMITTED;

    if (transport == NULL)
        return TIBRV_INVALID_ARG;

    if (cmName == NULL)
        return TIBRV_INVALID_ARG;

    status = tibrvcmTransport_CreateDistributedQueueEx(
                                &_cmTransport,
                                 transport->getHandle(),
                                 cmName,
                                 workerWeight,
                                 workerTasks,
                                 schedulerWeight,
                                 schedulerHeartbeat,
                                 schedulerActivation);
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
TibrvCmQueueTransport::isValid () const
{
    return ((_cmTransport != TIBRV_INVALID_ID) ? TIBRV_TRUE : TIBRV_FALSE);
}

void
TibrvCmQueueTransport_CompleteCB(tibrvcmTransport destroyedTransport,
                                 void* closure)
{
    USE_PARAM(destroyedTransport);

    if( !closure )
	return;

    TibrvCmQueueTransport *t = static_cast<TibrvCmQueueTransport *>(closure);
    TibrvCmQueueTransportOnComplete *cb = t->_completeCallback;

    if( cb )
	cb->onComplete(t);
}

TibrvStatus
TibrvCmQueueTransport::destroy()
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
TibrvCmQueueTransport::destroyEx(TibrvCmQueueTransportOnComplete* completeCB)
{
    tibrv_status status = TIBRV_INVALID_TRANSPORT;
    if (_cmTransport != TIBRV_INVALID_ID)
    {
	if( completeCB )
	{
	    _completeCallback = completeCB;
	    status = tibrvcmTransport_DestroyEx(_cmTransport,
                                                TibrvCmQueueTransport_CompleteCB,
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
TibrvCmQueueTransport::setCompleteTime(tibrv_f64 completeTime)
{
    return tibrvcmTransport_SetCompleteTime(_cmTransport,completeTime);
}

TibrvStatus
TibrvCmQueueTransport::getCompleteTime(tibrv_f64& completeTime) const
{
    return tibrvcmTransport_GetCompleteTime(_cmTransport,&completeTime);
}

TibrvStatus
TibrvCmQueueTransport::setWorkerWeight(tibrv_u32 workerWeight)
{
    return tibrvcmTransport_SetWorkerWeight(_cmTransport,workerWeight);
}

TibrvStatus
TibrvCmQueueTransport::getWorkerWeight(tibrv_u32& workerWeight) const
{
    return tibrvcmTransport_GetWorkerWeight(_cmTransport,&workerWeight);
}

TibrvStatus
TibrvCmQueueTransport::setWorkerTasks(tibrv_u32 workerTasks)
{
    return tibrvcmTransport_SetWorkerTasks(_cmTransport,workerTasks);
}

TibrvStatus
TibrvCmQueueTransport::getWorkerTasks(tibrv_u32& workerTasks) const
{
    return tibrvcmTransport_GetWorkerTasks(_cmTransport,&workerTasks);
}

TibrvStatus
TibrvCmQueueTransport::setTaskBacklogLimitInBytes(tibrv_u32 byteLimit)
{
    return tibrvcmTransport_SetTaskBacklogLimitInBytes(_cmTransport, byteLimit);
}

TibrvStatus
TibrvCmQueueTransport::setTaskBacklogLimitInMessages(tibrv_u32 messageLimit)
{
    return tibrvcmTransport_SetTaskBacklogLimitInMessages(_cmTransport, messageLimit);
}

TibrvStatus
TibrvCmQueueTransport::getUnassignedMessageCount(tibrv_u32& msgCount) const
{
    return tibrvcmTransport_GetUnassignedMessageCount(_cmTransport, &msgCount);
}

TibrvCmQueueTransport::TibrvCmQueueTransport(const TibrvCmQueueTransport& cmqtport) :
    TibrvCmTransport()
{
    _cmTransport = cmqtport.getHandle();
}

TibrvCmQueueTransport& TibrvCmQueueTransport::operator=(const TibrvCmQueueTransport& cmqtport)
{
    _cmTransport = cmqtport.getHandle();
    return *this;
}

//=====================================================================
