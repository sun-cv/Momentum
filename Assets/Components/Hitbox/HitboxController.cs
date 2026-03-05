using System;
using UnityEngine;



public class HitboxController : Controller
{
    readonly Color gizmoColor = Settings.Debug.GIZMO_COLOR;

    public Guid HitboxId     { get; private set; }
    public HitboxManager Manager { get; private set; }

    void OnTriggerEnter2D(Collider2D collision)
    {
        var actor = GetActor(collision);
        
        if (actor != null) 
            Manager.OnHitDetected(HitboxId, actor, CollisionPhase.Enter);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        var actor = GetActor(collision);
        if (actor != null) 
            Manager.OnHitDetected(HitboxId, actor, CollisionPhase.Stay);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        var actor = GetActor(collision);
        if (actor != null) 
            Manager.OnHitExited(HitboxId, actor);
    }

    Actor GetActor(Collider2D collision)
    {
        var view = collision.GetComponentInParent<BridgeController>();
        return view.Bridge.Owner;
    }

    public void Bind(HitboxManager manager, Guid id)
    {
        Manager  = manager;
        HitboxId = id;
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


