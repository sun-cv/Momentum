using System;
using UnityEngine;


namespace Momentum
{

public abstract class MetaBase
{
    public Guid  Id                     { get; }                = Guid.NewGuid();

    public float TimeCreated            { get; private set; }   = Time.time;
    public float TimeExecuted           { get; private set; }
    public float TimePaused             { get; private set; }
    public float TimeResumed            { get; private set; }
    public float TimeCancelled          { get; private set; }
    public float TimeInterrupted        { get; private set; }
    public float TimeCompleted          { get; private set; }



    public void MarkCreated()           => TimeCreated          = Time.time;
    public void MarkExecuted()          => TimeExecuted         = Time.time;
    public void MarkPaused()            => TimePaused           = Time.time;
    public void MarkResumed()           => TimeResumed          = Time.time;
    public void MarkCancelled()         => TimeCancelled        = Time.time;
    public void MarkInterrupted()       => TimeInterrupted      = Time.time;
    public void MarkCompleted()         => TimeCompleted        = Time.time;
}

public class Meta : MetaBase {}

public class RequestMeta : MetaBase
{
    public float TimeBuffered           { get; private set; }
    public float TimeValidated          { get; private set; }
    public float TimeInvalidated        { get; private set; }
    public float TimePending            { get; private set; }
    public float TimeExpired            { get; private set; }
    public float TimeResolved           { get; private set; }

    public void MarkBuffered()          => TimeBuffered         = Time.time;
    public void MarkValidated()         => TimeValidated        = Time.time;
    public void MarkInvalidated()       => TimeInvalidated      = Time.time;
    public void MarkPending()           => TimePending          = Time.time;
    public void MarkExpired()           => TimeExpired          = Time.time;
    public void MarkResolved()          => TimeResolved         = Time.time;
}

public class AbilityMeta : MetaBase
{
    public float TimeActivating         { get; private set; }
    public float TimeActivated          { get; private set; }
    public float TimeDeactivating       { get; private set; }
    public float TimeDeactivated        { get; private set; }

    public void MarkActivating()        => TimeActivating       = Time.time;       
    public void MarkActivated()         => TimeActivated        = Time.time;  
    public void MarkDeactivating()      => TimeDeactivating     = Time.time;  
    public void MarkDeactivated()       => TimeDeactivated      = Time.time;  

    public float TimeCastActivated      { get; private set; }
    public float TimeCastCancelled      { get; private set; }
    public float TimeCastInterrupted    { get; private set; }
    public float TimeCastCompleted      { get; private set; }

    public void MarkCastActivated()     => TimeCastActivated    = Time.time;  
    public void MarkCastCancelled()     => TimeCastCancelled    = Time.time;  
    public void MarkCastInterrupted()   => TimeCastInterrupted  = Time.time;  
    public void MarkCastCompleted()     => TimeCastCompleted    = Time.time;  
}



}