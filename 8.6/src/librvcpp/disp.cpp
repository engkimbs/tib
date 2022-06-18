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
// TibrvDispatcher
//=====================================================================

TibrvDispatcher::TibrvDispatcher()
{
    _dispatcher = TIBRV_INVALID_ID;
    _dispatchable = NULL;
}

TibrvDispatcher::~TibrvDispatcher()
{
    destroy();
}


TibrvStatus
TibrvDispatcher::create(TibrvDispatchable* dispatchable, tibrv_f64 timeout)
{
    if (_dispatcher != TIBRV_INVALID_ID)
        return TIBRV_NOT_PERMITTED;
    if (dispatchable == NULL)
        return TIBRV_INVALID_ARG;

    _dispatchable = dispatchable;

    return tibrvDispatcher_CreateEx(&_dispatcher,dispatchable->getDispatchable(),timeout);
}

TibrvStatus
TibrvDispatcher::destroy()
{
    if (_dispatcher == TIBRV_INVALID_ID)
        return TIBRV_INVALID_DISPATCHER;

    tibrv_status status = tibrvDispatcher_Destroy(_dispatcher);

    _dispatcher = TIBRV_INVALID_ID;
    _dispatchable = NULL;

    return status;
}

tibrv_bool
TibrvDispatcher::isValid() const
{
    return ((_dispatcher != TIBRV_INVALID_ID) ? TIBRV_TRUE : TIBRV_FALSE);
}

TibrvStatus
TibrvDispatcher::setName(const char* name)
{
    return tibrvDispatcher_SetName(_dispatcher,name);
}

TibrvStatus
TibrvDispatcher::getName(const char*& name) const
{
    return tibrvDispatcher_GetName(_dispatcher,&name);
}

TibrvDispatcher::TibrvDispatcher(const TibrvDispatcher& dispatcher)
{
    _dispatcher = dispatcher.getHandle();
}

TibrvDispatcher& TibrvDispatcher::operator=(const TibrvDispatcher& dispatcher)
{
    _dispatcher = dispatcher.getHandle();
    return *this;
}
