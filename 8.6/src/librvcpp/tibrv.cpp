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
// Tibrv
//=====================================================================

TibrvQueue Tibrv::_defaultQueue(TIBRV_DEFAULT_QUEUE);
TibrvProcessTransport Tibrv::_processTransport(TIBRV_PROCESS_TRANSPORT);

TibrvStatus
Tibrv::open()
{
    return tibrv_Open();
}

TibrvStatus
Tibrv::close()
{
    return tibrv_Close();
}

const char*
Tibrv::version()
{
    return tibrv_Version();
}
