using System;
using System.Collections.Generic;





public class TriggerLocks : RegisteredService
{

    readonly Dictionary<Capability, List<string>> locks = new();
    bool acceptingLocks = true;

    public override void Initialize()
    {
        Link.Global<Message<Request, MLock>>(HandleLockRequest);
    }

    void HandleLockRequest(Message<Request, MLock> message)
    {
        Response response = Response.Declined;       

        switch(message.Action)
        {
            case Request.Lock:                
                    if (!acceptingLocks)
                        break;
                    AddLock(message.Payload.Action, message.Payload.Origin);
                    response = Response.Accepted;
                break;

            case Request.Unlock:
                    RemoveLock(message.Payload.Action, message.Payload.Origin);
                    response = Response.Accepted;
                break;
            
            case Request.Enable:
                    SetAcceptingLocks(true);
                    response = Response.Accepted;
                break;
            
            case Request.Disable:
                    SetAcceptingLocks(false);
                    response = Response.Accepted;
                break;
        }

        Emit.Global<MLockResponse>(new(message.Id, response));
        Emit.Global(Publish.Changed, new MLocks(Snapshot.ReadOnly(locks)));
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
    void OnEvent<T>(T message) where T : IEvent     => EventBus<T>.Raise(message);

    public IReadOnlyDictionary<Capability, IReadOnlyList<string>> GetLocks() => Snapshot.ReadOnly(locks);
} 


public readonly struct MLock
{
    public Capability Action { get; init; }
    public string Origin     { get; init; }

    public MLock(Capability action, string origin)
    {
        Action  = action;
        Origin  = origin;
    }
}

public readonly struct MLocks
{
    public IReadOnlyDictionary<Capability, IReadOnlyList<string>> Locks { get;}

    public MLocks(IReadOnlyDictionary<Capability, IReadOnlyList<string>> locks)
    {
        Locks   = locks;
    }
}

public readonly struct MLockResponse: ISystemEvent
{
    public readonly Guid Id             { get; }
    public readonly Response Response   { get; }

    public MLockResponse(Guid id, Response response)
    {
        Id          = id;
        Response    = response;
    }
}