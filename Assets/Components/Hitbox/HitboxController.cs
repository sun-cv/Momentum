using System;
using System.Collections.Generic;
using UnityEngine;



public class HitboxController : Controller
{
    readonly Color gizmoColor = Settings.Debug.GIZMO_COLOR;

    public Actor Owner                              { get; set; }
    public HitboxManager Manager                    { get; set; }

    public Guid HitboxId                            { get; set; }
    public object Package                           { get; set; }
    public HitboxDefinition Definition              { get; set; }

    private HashSet<Actor> hitActors                = new();
    private Dictionary<Actor, float> nextHitTime    = new();

    private void OnTriggerEnter2D(Collider2D collision)
    {

        var controller = collision.GetComponentInParent<BridgeController>();
        if (controller == null) return;

        var target = controller.Bridge.Owner;

        if (ShouldHit(target))
        {
            SendHitEvent(target);
            RecordHit(target);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!Definition.Behavior.AllowMultiHit) 
        {
            return;
        }

        var controller = collision.GetComponentInParent<BridgeController>();

        if (controller == null) 
        {
            return;
        }

        var target = controller.Bridge.Owner;

        if (ShouldHit(target))
        {
            SendHitEvent(target);
            RecordHit(target);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var controller  = collision.GetComponentInParent<BridgeController>();

        if (controller  == null) 
        {
            return;
        }

        var target      = controller.Bridge.Owner;

        nextHitTime.Remove(target);
    }

    bool ShouldHit(Actor target)
    {
        if (target == Owner) return false;

        if (!Definition.Behavior.AllowMultiHit)
        {
            return !hitActors.Contains(target);
        }

        if (nextHitTime.TryGetValue(target, out float nextTime))
        {
            return Clock.Time >= nextTime;
        }

        return true;
    }

    void RecordHit(Actor target)
    {
        if (!Definition.Behavior.AllowMultiHit)
        {
            hitActors.Add(target);
        }
        else
        {
            nextHitTime[target] = Time.time + Definition.Behavior.MultiHitInterval;
        }
    }

    void SendHitEvent(Actor target)
    {    
        Emit.Global(Request.Queue, new TriggerEvent() { Source = Owner, Target = target, Package = Package });
    }

    public void Bind(HitboxManager manager, Actor owner, Guid id, HitboxDefinition definition, object package)
    {
        if (Owner != null) return;

        Manager     = manager;
        Owner       = owner;
        HitboxId    = id;
        Package     = package;
        Definition  = definition;
    }

    
    private void OnDrawGizmos()
    {
        if (!HitboxManager.ShowDebugGizmos) return;
        
        Gizmos.color = gizmoColor;
        DrawColliders();
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!HitboxManager.ShowDebugGizmos) return;

        Gizmos.color = Color.yellow;
        DrawColliders();
    }
    
    void DrawColliders()
    {
        var colliders = GetComponentsInChildren<Collider2D>();

        foreach (var collider in colliders)
            GizmoTools.DrawCollider(collider, true);
    }
}


