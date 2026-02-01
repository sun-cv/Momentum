using System;
using UnityEngine;



public enum DamageType
{
    
}


public class CombatLogic
{
    





}






public readonly struct CombatRequestPayload
{
    public Actor Target                     { get; init; }
    public Actor Source                     { get; init; }

    public DamageType DamageType            { get; init; }
    public float Force                      { get; init; }
    public Vector2 Direction                { get; init; }
}


public readonly struct CombatRequest : ISystemEvent
{
    public Guid Id                          { get; }
    public Publish Action                   { get; }
    public CombatRequestPayload Payload { get; }

    public CombatRequest(Guid id, Publish action, CombatRequestPayload payload)
    {
        Id      = id;
        Action  = action;
        Payload = payload;
    }
}








public readonly struct KillingBlowPublish : ISystemEvent
{
    public Guid Id                          { get; }
    public Publish Action                   { get; }
    public CombatRequest Payload            { get; }

    public KillingBlowPublish(Guid id, Publish action, CombatRequest payload)
    {
        Id      = id;
        Action  = action;
        Payload = payload;
    }
}