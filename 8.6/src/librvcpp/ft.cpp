/*
 * Copyright (c) 1995-$Date: 2016-12-13 12:21:58 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
 * All Rights Reserved. Confidential & Proprietary.
 * TIB/Rendezvous is protected under US Patent No. 5,187,787.
 * For more information, please contact:
 * TIBCO Software, Inc., Palo Alto, California, USA
 *
 */


#include "ftcpp.h"


//=====================================================================
// TibrvFtMember
//=====================================================================

void TibrvFtMember_CompleteCB(tibrvftMember ftMember, void *closure)
{
    USE_PARAM(ftMember);

    if (closure == NULL)
        return;

    TibrvFtMember * member = static_cast<TibrvFtMember*>(closure);
    TibrvFtMemberOnComplete * cb = member->_completeCallback;
    if (cb != NULL)
        cb->onComplete(member);
}

void TibrvFtMember_FtMemberCB(tibrvftMember ftMember, const char* groupName,
                 tibrvftAction action, void *closure)
{
    USE_PARAM(ftMember);

    if (closure == NULL)
        return;

    TibrvFtMember * pmember = static_cast<TibrvFtMember*>(closure);
    TibrvFtMemberCallback * cb = pmember->_callback;
    if (cb != NULL)
        cb->onFtAction(pmember,groupName,action);
}

TibrvFtMember::TibrvFtMember()
{
    _ftMember = TIBRV_INVALID_ID;
    _queue = NULL;
    _transport = NULL;
    _callback = NULL;
    _completeCallback = NULL;
    _closure  = NULL;
}

TibrvFtMember::~TibrvFtMember()
{
    destroy();
}

TibrvStatus
TibrvFtMember::create(TibrvQueue* queue,
                      TibrvFtMemberCallback *callback,
                      TibrvTransport* transport,
                      const char* groupName,
                      tibrv_u16  weight,
                      tibrv_u16  activeGoal,
                      tibrv_f64  heartbeatInterval,
                      tibrv_f64  preparationInterval,
                      tibrv_f64  activationInterval,
                      const void* closure)
{
    if (_ftMember != TIBRV_INVALID_ID) return TIBRV_NOT_PERMITTED;
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

    if (transport != NULL)
    {
        _transport = transport;
        t = _transport->getHandle();
    }
    else
        return TIBRV_INVALID_TRANSPORT;

    _callback = callback;
    _closure = const_cast<void*>(closure);
    status = tibrvftMember_Create(&_ftMember,q,
                                    TibrvFtMember_FtMemberCB,
                                    t,groupName,weight,
                                    activeGoal,heartbeatInterval,
                                    preparationInterval,activationInterval,
                                    this);
    if (status != TIBRV_OK)
        _ftMember = TIBRV_INVALID_ID;

    return status;
}

TibrvStatus
TibrvFtMember::destroy(TibrvFtMemberOnComplete* completeCB)
{
    tibrv_status status = TIBRV_INVALID_EVENT;
    if (_ftMember != TIBRV_INVALID_ID)
    {
        if (completeCB == NULL)
        {
            status = tibrvftMember_Destroy(_ftMember);
        }
        else
        {
            _completeCallback = completeCB;
            status = tibrvftMember_DestroyEx(_ftMember, TibrvFtMember_CompleteCB);
        }
        _ftMember = TIBRV_INVALID_ID;
    }
    return status;
}

tibrv_bool
TibrvFtMember::isValid() const
{
    return ((_ftMember != TIBRV_INVALID_ID) ? TIBRV_TRUE : TIBRV_FALSE);
}

TibrvStatus
TibrvFtMember::setWeight(tibrv_u16 weight)
{
    return tibrvftMember_SetWeight(_ftMember,weight);
}

TibrvStatus
TibrvFtMember::getWeight (tibrv_u16& weight) const
{
    return tibrvftMember_GetWeight(_ftMember,&weight);
}

TibrvStatus
TibrvFtMember::getGroupName(const char *& groupName) const
{
    return tibrvftMember_GetGroupName(_ftMember,&groupName);
}

TibrvFtMember::TibrvFtMember(const TibrvFtMember& ftMember)
{
    _ftMember = ftMember.getHandle();
}

TibrvFtMember& TibrvFtMember::operator=(const TibrvFtMember& ftMember)
{
    _ftMember = ftMember.getHandle();
    return *this;
}

//=====================================================================
// TibrvFtMonitor
//=====================================================================

void TibrvFtMonitor_CompleteCB(tibrvftMonitor ftMonitor, void *closure)
{
    USE_PARAM(ftMonitor);

    if (closure == NULL)
        return;

    TibrvFtMonitor * monitor = static_cast<TibrvFtMonitor*>(closure);
    TibrvFtMonitorOnComplete * cb = monitor->_completeCallback;
    if (cb != NULL)
        cb->onComplete(monitor);
}

void TibrvFtMonitor_FtMonitorCB(tibrvftMonitor ftMonitor, const char* groupName,
                                tibrv_u32 numActiveMembers, void *closure)
{
    USE_PARAM(ftMonitor);

    if (closure == NULL)
        return;

    TibrvFtMonitor * pmonitor = static_cast<TibrvFtMonitor*>(closure);
    TibrvFtMonitorCallback * cb = pmonitor->_callback;
    if (cb != NULL)
        cb->onFtMonitor(pmonitor,groupName,numActiveMembers);
}

TibrvFtMonitor::TibrvFtMonitor()
{
    _ftMonitor = TIBRV_INVALID_ID;
    _queue = NULL;
    _transport = NULL;
    _callback = NULL;
    _completeCallback = NULL;
    _closure  = NULL;
}

TibrvFtMonitor::~TibrvFtMonitor()
{
    destroy();
}

TibrvStatus
TibrvFtMonitor::create(TibrvQueue* queue,
                       TibrvFtMonitorCallback *callback,
                       TibrvTransport* transport,
                       const char* groupName,
                       tibrv_f64 lostInterval,
                       const void *closure)
{
    if (_ftMonitor != TIBRV_INVALID_ID) return TIBRV_NOT_PERMITTED;

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

    if (transport != NULL)
    {
        _transport = transport;
        t = _transport->getHandle();
    }
    else
        return TIBRV_INVALID_TRANSPORT;

    _callback = callback;
    _closure = const_cast<void*>(closure);

    status = tibrvftMonitor_Create(&_ftMonitor,q,
                                   TibrvFtMonitor_FtMonitorCB,
                                   t,groupName,lostInterval,this);
    if (status != TIBRV_OK)
        _ftMonitor = TIBRV_INVALID_ID;

    return status;
}

TibrvStatus
TibrvFtMonitor::destroy(TibrvFtMonitorOnComplete* completeCB)
{
    tibrv_status status = TIBRV_INVALID_EVENT;
    if (_ftMonitor != TIBRV_INVALID_ID)
    {
        if (completeCB == NULL)
        {
            status = tibrvftMonitor_Destroy(_ftMonitor);
        }
        else
        {
            _completeCallback = completeCB;
            status = tibrvftMonitor_DestroyEx(_ftMonitor, TibrvFtMonitor_CompleteCB);
        }
        _ftMonitor = TIBRV_INVALID_ID;
    }
    return status;
}

tibrv_bool
TibrvFtMonitor::isValid() const
{
    return ((_ftMonitor != TIBRV_INVALID_ID) ? TIBRV_TRUE : TIBRV_FALSE);
}

TibrvStatus
TibrvFtMonitor::getGroupName(const char*& queueName) const
{
    return tibrvftMonitor_GetGroupName(_ftMonitor,&queueName);
}

TibrvFtMonitor::TibrvFtMonitor(const TibrvFtMonitor& ftMonitor)
{
    _ftMonitor = ftMonitor.getHandle();
}

TibrvFtMonitor& TibrvFtMonitor::operator=(const TibrvFtMonitor& ftMonitor)
{
    _ftMonitor = ftMonitor.getHandle();
    return *this;
}

//=====================================================================
