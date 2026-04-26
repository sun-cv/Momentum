using System;
using System.Collections.Generic;





public class TriggerLocks : RegisteredService, IInitialize
{

    readonly Dictionary<Trigger, List<string>> locks = new();

            // -----------------------------------

    bool acceptingLocks = true;

    // ===============================================================================

    public void Initialize()
    {
        Link.Global<LockEvent>(HandleLockRequest);
    }

    // ===============================================================================

    void AddLock(Trigger action, string origin)
    {
        if (!locks.TryGetValue(action, out var list))
        {   
            list = new();
            locks[action] = list;
        }
        
        list.Add(origin);
    }

    void RemoveLock(Trigger action, string origin)
    {
        if (!locks.TryGetValue(action, out var list))
            return;
        
        list.Remove(origin);
    }

    public void SetAcceptingLocks(bool value)
    {
        acceptingLocks = value;
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleLockRequest(LockEvent message)
    {
        switch(message.Request)
        {
            case Request.Lock:                
                    if (!acceptingLocks)
                        break;
                    AddLock(message.Action, message.Origin);
                break;

            case Request.Unlock:
                    RemoveLock(message.Action, message.Origin);
                break;
            
            case Request.Enable:
                    SetAcceptingLocks(true);
                break;
            
            case Request.Disable:
                    SetAcceptingLocks(false);
                break;
        }

        Emit.Global(new LockUpdateEvent(Snapshot.ReadOnly(locks)));
    }


    public bool IsLocked(Trigger action)     => locks.TryGetValue(action, out var list) && list.Count > 0;

    // ===============================================================================

    public IReadOnlyDictionary<Trigger, IReadOnlyList<string>> GetLocks() => Snapshot.ReadOnly(locks);
} 


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct LockEvent : IMessage
{
    public readonly Trigger Action       { get; init; }
    public readonly string     Origin       { get; init; }
    public readonly Request    Request      { get; init; }

    public LockEvent(Trigger action, string origin, Request request)
    {
        Action  = action;
        Origin  = origin;
        Request = request;
    }
}

public readonly struct LockUpdateEvent : IMessage
{
    public IReadOnlyDictionary<Trigger, IReadOnlyList<string>> Locks { get;}

    public LockUpdateEvent(IReadOnlyDictionary<Trigger, IReadOnlyList<string>> locks)
    {
        Locks   = locks;
    }
}
