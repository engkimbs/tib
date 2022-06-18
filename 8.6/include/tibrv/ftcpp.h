/*
 * Copyright (c) 1995-$Date: 2016-12-13 12:21:58 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
 * All Rights Reserved. Confidential & Proprietary.
 * TIB/Rendezvous is protected under US Patent No. 5,187,787.
 * For more information, please contact:
 * TIBCO Software, Inc., Palo Alto, California, USA
 *
 */

#ifndef _INCLUDED_tibrvftcpp_h
#define _INCLUDED_tibrvftcpp_h

#include "tibrvcpp.h"
#include "ft.h"

//=====================================================================
// Forward declarations
//=====================================================================

class TibrvFtMember;
class TibrvFtMonitor;

class TibrvFtMemberCallback;
class TibrvFtMonitorCallback;

class TibrvFtMemberOnComplete;
class TibrvFtMonitorOnComplete;

//=====================================================================
// TibrvFtMemberCallback
//=====================================================================

class TibrvFtMemberCallback {

public:
    TibrvFtMemberCallback(){};
    virtual ~TibrvFtMemberCallback(){};

    virtual void onFtAction(TibrvFtMember * ftMember,
                            const char* groupName,
                            tibrvftAction action ) = 0;

};

//=====================================================================
// TibrvFtMonitorCallback
//=====================================================================

class TibrvFtMonitorCallback {

public:
    TibrvFtMonitorCallback(){};
    virtual ~TibrvFtMonitorCallback(){};

    virtual void onFtMonitor(TibrvFtMonitor * ftMonitor,
                             const char* groupName,
                             tibrv_u32 numActiveMembers ) = 0;

};

//=====================================================================
// TibrvFtMemberOnComplete
//=====================================================================

class TibrvFtMemberOnComplete {

public:
    TibrvFtMemberOnComplete(){};
    virtual ~TibrvFtMemberOnComplete(){};

    virtual void onComplete(TibrvFtMember * ftMember) = 0;
};

//=====================================================================
// TibrvFtMonitorOnComplete
//=====================================================================

class TibrvFtMonitorOnComplete {

public:
    TibrvFtMonitorOnComplete(){};
    virtual ~TibrvFtMonitorOnComplete(){};

    virtual void onComplete(TibrvFtMonitor * ftMonitor) = 0;
};

//=====================================================================
// TibrvFtMember
//=====================================================================

extern "C" void TibrvFtMember_FtMemberCB(tibrvftMember ftMember,
                                         const char* groupName,
                                         tibrvftAction action,
                                         void *closure);
extern "C" void TibrvFtMember_CompleteCB(tibrvftMember event, void *closure);

class TibrvFtMember {

    friend void TibrvFtMember_FtMemberCB(tibrvftMember ftMember,
                                         const char* groupName,
                                         tibrvftAction action,
                                         void *closure);
    friend void TibrvFtMember_CompleteCB(tibrvftMember event, void *closure);

public:

    TibrvFtMember();
    virtual ~TibrvFtMember();

    TibrvStatus create(TibrvQueue* queue,
                       TibrvFtMemberCallback* callback,
                       TibrvTransport* transport,
                       const char* groupName,
                       tibrv_u16  weight,
                       tibrv_u16  activeGoal,
                       tibrv_f64  heartbeatInterval,
                       tibrv_f64  preparationInterval,
                       tibrv_f64  activationInterval,
                       const void* closure = NULL);

    // called automatically by destructor
    TibrvStatus destroy  (TibrvFtMemberOnComplete* completeCB = NULL);
    tibrv_bool  isValid  () const;

    void*       getClosure() const;

    TibrvQueue*     getQueue     () const;
    TibrvTransport* getTransport () const;
    TibrvStatus     getGroupName(const char *& groupName) const;

    TibrvStatus setWeight (tibrv_u16  weight);
    TibrvStatus getWeight (tibrv_u16& weight) const;

    tibrvftMember  getHandle() const { return _ftMember; }

private:

    tibrvftMember _ftMember;

    TibrvQueue*     _queue;
    TibrvTransport* _transport;
    TibrvFtMemberCallback* _callback;
    TibrvFtMemberOnComplete * _completeCallback;
    void* _closure;

    TibrvFtMember(const TibrvFtMember& ftMember);
    TibrvFtMember& operator=(const TibrvFtMember& ftMember);

};


//=====================================================================
// TibrvFtMonitor
//=====================================================================

extern "C" void TibrvFtMonitor_FtMonitorCB(tibrvftMonitor monitor,
                                           const char* groupName,
                                           tibrv_u32 numActiveMembers,
                                           void* closure);
extern "C" void TibrvFtMonitor_CompleteCB(tibrvftMonitor event, void *closure);

class TibrvFtMonitor {

    friend void TibrvFtMonitor_FtMonitorCB(tibrvftMonitor monitor,
                                           const char* groupName,
                                           tibrv_u32 numActiveMembers,
                                           void* closure);
    friend void TibrvFtMonitor_CompleteCB(tibrvftMonitor event, void *closure);

public:

    TibrvFtMonitor();
    virtual ~TibrvFtMonitor();

    TibrvStatus create  (TibrvQueue* queue,
                         TibrvFtMonitorCallback *callback,
                         TibrvTransport* transport,
                         const char* groupName,
                         tibrv_f64  lostInterval,
                         const void* closure = NULL);

    // called automatically by destructor
    TibrvStatus destroy  (TibrvFtMonitorOnComplete* completeCB=NULL);
    tibrv_bool  isValid  () const;

    void*       getClosure() const;

    TibrvQueue*     getQueue     () const;
    TibrvTransport* getTransport () const;
    TibrvStatus     getGroupName(const char *& groupName) const;

    tibrvftMonitor  getHandle() const
                        {return _ftMonitor;}

private:

    tibrvftMonitor _ftMonitor;

    TibrvQueue*     _queue;
    TibrvTransport* _transport;
    TibrvFtMonitorCallback* _callback;
    TibrvFtMonitorOnComplete * _completeCallback;
    void* _closure;

    TibrvFtMonitor(const TibrvFtMonitor& ftMonitor);
    TibrvFtMonitor& operator=(const TibrvFtMonitor& ftMonitor);

};


//=====================================================================
// inline implementations
//=====================================================================

_TIBRV_INLINE TibrvQueue* TibrvFtMember::getQueue() const   { return _queue; }
_TIBRV_INLINE TibrvTransport* TibrvFtMember::getTransport() const  { return _transport; }
_TIBRV_INLINE void* TibrvFtMember::getClosure() const  { return _closure; }

_TIBRV_INLINE TibrvQueue* TibrvFtMonitor::getQueue() const  { return _queue; }
_TIBRV_INLINE TibrvTransport* TibrvFtMonitor::getTransport() const  { return _transport; }
_TIBRV_INLINE void* TibrvFtMonitor::getClosure() const  { return _closure; }

//=====================================================================

#endif  /* _INCLUDED_tibrvftcpp_h */
