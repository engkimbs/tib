/*
 * Copyright (c) 1998-$Date: 2016-12-13 12:17:30 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
 * All Rights Reserved. Confidential & Proprietary.
 * TIB/Rendezvous is protected under US Patent No. 5,187,787.
 * For more information, please contact:
 * TIBCO Software Inc., Palo Alto, California, USA
 *
 */

#ifndef _INCLUDED_tibrv_h
#define _INCLUDED_tibrv_h

#include <stdio.h>
#include <time.h>

#include "types.h"
#include "events.h"
#include "status.h"
#include "msg.h"
#include "queue.h"
#include "qgroup.h"
#include "tport.h"
#include "disp.h"


#if defined(__cplusplus)
extern "C" {
#endif

extern const char*
tibrv_Version(void);

/* Initialization */
extern tibrv_status
tibrv_Open(void);

extern tibrv_status
tibrv_Close(void);

/* for EBCDIC systems this call sets the iconv conversion code page
   or CCSID.  On all other systems this is a no op. */
extern tibrv_status
tibrv_SetCodePages(
    char *host_codepage,
    char *net_codepage);

extern tibrv_status
tibrv_SetRVParameters(
    tibrv_u32   argc,
    const char  **argv);

extern tibrv_status
tibrv_OpenEx(
    const char  *pathname);

extern tibrv_bool
tibrv_IsIPM(void);
    
#ifdef  __cplusplus
}
#endif

#endif /* _INCLUDED_tibrv_h */
