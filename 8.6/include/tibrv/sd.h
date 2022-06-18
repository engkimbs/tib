/*
 * Copyright (c) 1998-$Date: 2016-12-13 12:24:21 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
 * All Rights Reserved. Confidential & Proprietary.
 * TIB/Rendezvous is protected under US Patent No. 5,187,787.
 * For more information, please contact:
 * TIBCO Software Inc., Palo Alto, California, USA
 *
 */

#ifndef _INCLUDED_tibrvsd_h
#define _INCLUDED_tibrvsd_h

#ifdef __VMS
  #include "rvvms.h"
#endif

#include "types.h"
#include "status.h"

#if defined(__cplusplus)
extern "C" {
#endif

#define TIBRV_SECURE_DAEMON_ANY_NAME            (NULL)
#define TIBRV_SECURE_DAEMON_ANY_CERT            (NULL)

extern tibrv_status
tibrvSecureDaemon_SetDaemonCert(
    const char*                 daemonName,
    const char*                 daemonCert);

extern tibrv_status
tibrvSecureDaemon_SetUserCertWithKey(
    const char*                 userCertWithKey,
    const char*                 password);

extern tibrv_status
tibrvSecureDaemon_SetUserCertWithKeyBin(
    const void*                 userCertWithKey,
    tibrv_u32			userCertWithKey_size,
    const char*                 password);

extern tibrv_status
tibrvSecureDaemon_SetUserNameWithPassword(
    const char*                 userName,
    const char*                 password);


#if defined(__cplusplus)
}
#endif

#endif /* _INCLUDED_tibrvsd_h */
