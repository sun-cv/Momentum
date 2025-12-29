using System;
using System.Collections.Generic;
using UnityEngine;





public class ActionLocks : RegisteredService
{

    readonly Dictionary<Capability, List<string>> locks = new();
    bool acceptingLocks = true;

    public override void Initialize()
    {
        EventBus<LockRequest>.Subscribe(HandleLockRequest);
    }

    void HandleLockRequest(LockRequest evt)
    {
        Response response = Response.Declined;       

        switch(evt.Action)
        {
            case LockAction.Lock:                
                    if (!acceptingLocks)
                        break;
                    AddLock(evt.Payload.Action, evt.Payload.Origin);
                    response = Response.Accepted;
                break;

            case LockAction.Unlock:
                    RemoveLock(evt.Payload.Action, evt.Payload.Origin);
                    response = Response.Accepted;
                break;
            
            case LockAction.EnableRequests:
                    SetAcceptingLocks(true);
                    response = Response.Accepted;
                break;
            
            case LockAction.DisableRequests:
                    SetAcceptingLocks(false);
                    response = Response.Accepted;
                break;
        }

        OnEvent<LockResponse>(new(evt.Id, response));
        OnEvent<LockPublish>(new(Guid.NewGuid(), Publish.Changed, new(){ Locks = Snapshot.ReadOnly(locks) }));
    }


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

    public bool IsLocked(Capability action)     => locks.TryGetValue(action, out var list) && list.Count > 0;
    public void SetAcceptingLocks(bool value)   => acceptingLocks = value;
    void OnEvent<T>(T evt) where T : IEvent     => EventBus<T>.Raise(evt);

    public IReadOnlyDictionary<Capability, IReadOnlyList<string>> GetLocks() => Snapshot.ReadOnly(locks);
} 

public enum LockAction
{
    Lock,
    Unlock,
    EnableRequests,
    DisableRequests,
}

public readonly struct LockRequestPayload
{
    public Capability Action { get; init; }
    public string Origin     { get; init; }
}

public readonly struct LockStatePayload
{
    public IReadOnlyDictionary<Capability, IReadOnlyList<string>> Locks { get; init; }
}

public readonly struct LockRequest  : IEventRequest
{ 
    public Guid Id                      { get; }
    public LockAction Action            { get; }
    public LockRequestPayload Payload   { get; }

    public LockRequest(Guid id, LockAction action, LockRequestPayload payload) 
    { 
        Id      = id;
        Action  = action; 
        Payload = payload;
    }
}
public readonly struct LockResponse : IEventResponse
{ 
    public Guid Id                      { get; }
    public Response Action              { get; }

    public LockResponse(Guid id, Response action) 
    { 
        Id       = id;
        Action   = action; 
    }
}

public readonly struct LockPublish  : IEventPublish
{
    public Guid Id                      { get; }
    public Publish Action               { get; }
    public LockStatePayload Payload     { get; }

    public LockPublish(Guid id, Publish action, LockStatePayload payload) 
    { 
        Id      = id;
        Action  = action; 
        Payload = payload;
    }
}