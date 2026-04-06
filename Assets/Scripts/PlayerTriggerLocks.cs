using System;
using System.Collections.Generic;





public class TriggerLocks : RegisteredService, IInitialize
{

    readonly Dictionary<Capability, List<string>> locks = new();

            // -----------------------------------

    bool acceptingLocks = true;

    // ===============================================================================

    public void Initialize()
    {
        Link.Global<LockEvent>(HandleLockRequest);
    }

    // ===============================================================================

    void AddLock(Capability action, string origin)
    {
        if (!locks.TryGetValue(action, out var list))
        {   
            list = new();
            locks[action] = list;
        }
        
        list.Add(origin);
    }

    void RemoveLock(Capability action, string origin)
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


    public bool IsLocked(Capability action)     => locks.TryGetValue(action, out var list) && list.Count > 0;

    // ===============================================================================

    public override void Dispose()
    {
        // NO OP;
    }

    public IReadOnlyDictionary<Capability, IReadOnlyList<string>> GetLocks() => Snapshot.ReadOnly(locks);
} 


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct LockEvent : IMessage
{
    public readonly Capability Action       { get; init; }
    public readonly string     Origin       { get; init; }
    public readonly Request    Request      { get; init; }

    public LockEvent(Capability action, string origin, Request request)
    {
        Action  = action;
        Origin  = origin;
        Request = request;
    }
}

public readonly struct LockUpdateEvent : IMessage
{
    public IReadOnlyDictionary<Capability, IReadOnlyList<string>> Locks { get;}

    public LockUpdateEvent(IReadOnlyDictionary<Capability, IReadOnlyList<string>> locks)
    {
        Locks   = locks;
    }
}
