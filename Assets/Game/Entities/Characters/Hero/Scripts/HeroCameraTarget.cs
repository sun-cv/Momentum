using System;
using UnityEngine;



public readonly struct ActorCameraTarget : IAdvancedCameraTarget
{
    public WeakReference<Actor> ActorRef { get; init; }
    
    public readonly bool IsValid
    {
        get
        {
            if (ActorRef == null || !ActorRef.TryGetTarget(out var actor))
                return false;
                
            return actor.Bridge?.View != null && actor.Bridge.View;
        }
    }
    
    public readonly Vector3 GetPosition()
    {
        if (!ActorRef.TryGetTarget(out var actor) || !actor.Bridge.View)
            return Vector3.zero;
            
        return actor.Bridge.View.transform.position;
    }
    
    public readonly bool IsMoving
    {
        get
        {
            if (!ActorRef.TryGetTarget(out var actor))
                return false;
            return actor is IMovable instance && instance.IsMoving;
        }
    }
    
    public readonly Vector2 Velocity
    {
        get
        {
            if (!ActorRef.TryGetTarget(out var actor))
                return Vector2.zero;
            return actor is IDynamic instance ? instance.Velocity : Vector2.zero;
        }
    }
    
    public readonly TimePredicate Idle
    {
        get
        {
            if (!ActorRef.TryGetTarget(out var actor))
                return null;
            return actor is IIdle instance ? instance.IsIdle : null;
        }
    }

    public ActorCameraTarget(Actor actor)
    {
        ActorRef = new WeakReference<Actor>(actor);
    }
}