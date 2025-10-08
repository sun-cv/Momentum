using System;
using Unity.VisualScripting;
using UnityEngine;


namespace Momentum
{

public abstract class MetaBase
{
    public Guid Id                      { get; }                = Guid.NewGuid();
    public Status Status                { get; protected set; } = Status.None;

    public float TimeCreated            { get; private   set; }
    public void MarkCreated()           { TimeCreated = Time.time; }
}

public class Meta : MetaBase {}

public class RequestMeta : Meta
{
    public Request  request             { get; private set; }
    public Response response            { get; private set; }

    public float Accepted               { get; private set; }
    public float Rejected               { get; private set; }
    public float Pending                { get; private set; }
    public float Expired                { get; private set; }

    public void MarkAccepted()          { Accepted = Time.time; response = Response.Accepted; } 
    public void MarkRejected()          { Rejected = Time.time; response = Response.Rejected; } 
    public void MarkPending()           { Pending  = Time.time; response = Response.Pending;  } 
    public void MarkExpired()           { Expired  = Time.time; response = Response.Expired;  }
}

public class LifecycleMeta : Meta
{
    public Lifecycle Lifecycle          { get; private set; }

    public float Queued                 { get; private set; }   
    public float Running                { get; private set; }   
    public float Paused                 { get; private set; }   
    public float Completed              { get; private set; }   
    public float Failed                 { get; private set; }   
    public float Cancelled              { get; private set; }   
    public float Interrupted            { get; private set; }   

    public void MarkQueued()            { Queued      = Time.time; Lifecycle = Lifecycle.Queued;      Status = Status.Inactive; }         
    public void MarkRunning()           { Running     = Time.time; Lifecycle = Lifecycle.Running;     Status = Status.Active;   }    
    public void MarkPaused()            { Paused      = Time.time; Lifecycle = Lifecycle.Paused;      Status = Status.Disabled; }    
    public void MarkCompleted()         { Completed   = Time.time; Lifecycle = Lifecycle.Completed;   Status = Status.Inactive; }    
    public void MarkFailed()            { Failed      = Time.time; Lifecycle = Lifecycle.Failed;      Status = Status.Disabled; }    
    public void MarkCancelled()         { Cancelled   = Time.time; Lifecycle = Lifecycle.Cancelled;   Status = Status.Disabled; }    
    public void MarkInterrupted()       { Interrupted = Time.time; Lifecycle = Lifecycle.Interrupted; Status = Status.Disabled; }    
}



}