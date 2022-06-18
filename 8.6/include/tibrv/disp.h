/*
 * Copyright (c) 1998-$Date: 2016-12-13 12:17:30 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
 * All Rights Reserved. Confidential & Proprietary.
 * TIB/Rendezvous is protected under US Patent No. 5,187,787.
 * For more information, please contact:
 * TIBCO Software Inc., Palo Alto, California, USA
 *
 */

#ifndef _INCLUDED_tibrvdisp_h
#define _INCLUDED_tibrvdisp_h

#include "types.h"
#include "status.h"

#if defined(__cplusplus)
extern "C" {
#endif

#define \
tibrvDispatcher_Create(disp, dispactchable) \
    tibrvDispatcher_CreateEx(disp, dispactchable, TIBRV_WAIT_FOREVER)

extern tibrv_status
tibrvDispatcher_CreateEx(
    tibrvDispatcher*            dispatcher,
    tibrvDispatchable           dispatchable,
    tibrv_f64                   idleTimeout);

extern tibrv_status
tibrvDispatcher_Join(
    tibrvDispatcher             dispatcher);

extern tibrv_status
tibrvDispatcher_Destroy(
    tibrvDispatcher             dispatcher);

extern tibrv_status
tibrvDispatcher_SetName(
    tibrvDispatcher             dispatcher,
    const char*                 dispatchName);

extern tibrv_status
tibrvDispatcher_GetName(
    tibrvDispatcher             dispatcher,
    const char**                dispatchName);


#ifdef  __cplusplus
}
#endif

#endif /* _INCLUDED_tibrvdisp_h */
