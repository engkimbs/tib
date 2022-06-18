/*
 * Copyright (c) 1995-$Date: 2016-12-13 12:21:58 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
 * All Rights Reserved. Confidential & Proprietary.
 * TIB/Rendezvous is protected under US Patent No. 5,187,787.
 * For more information, please contact:
 * TIBCO Software, Inc., Palo Alto, California, USA
 *
 */

#include "sdcpp.h"

//=====================================================================
// TibrvSdContext
//=====================================================================

TibrvStatus
TibrvSdContext::setDaemonCert(const char* daemonName, 
                              const char* daemonCert)
{
    return tibrvSecureDaemon_SetDaemonCert(daemonName, daemonCert);
}

TibrvStatus
TibrvSdContext::setUserCertWithKey(const char* userCertWithKey, 
                                   const char* password)
{
    return tibrvSecureDaemon_SetUserCertWithKey(userCertWithKey, password);
}

TibrvStatus
TibrvSdContext::setUserCertWithKeyBin(const void* userCertWithKey,
                                      tibrv_u32	  userCertWithKey_size,
                                      const char* password)
{
    return tibrvSecureDaemon_SetUserCertWithKeyBin(userCertWithKey, userCertWithKey_size, password);
}

TibrvStatus
TibrvSdContext::setUserNameWithPassword(const char* userName, 
                                        const char* password)
{
    return tibrvSecureDaemon_SetUserNameWithPassword(userName, password);
}

//=====================================================================
