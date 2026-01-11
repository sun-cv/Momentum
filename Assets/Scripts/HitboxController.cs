using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class HitboxController : Controller
{
    public int HitboxId             { get; set; }
    public HitboxManager Manager    { get; set; }
    public Actor Owner              { get; set; }
    
    readonly Color gizmoColor        = Color.red;

    private void OnTriggerEnter(Collider trigger)
    {
        if (!trigger.TryGetComponent<BridgeController>(out var target))
            return;

        Manager.DetectHit(new(){ HitboxId = HitboxId, Owner = Owner, Target = target.Bridge.Owner });
    }

    public void Configure(int Id, Actor owner, HitboxManager manager)
    {
        if (Owner != null)
            return;

        HitboxId    = Id;
        Owner       = owner;
        Manager     = manager;
    }
    
    private void OnDrawGizmos()
    {

        if (!HitboxManager.ShowDebugGizmos) return;
        
        Gizmos.color = gizmoColor;
        DrawColliders();
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        DrawColliders();
    }
    
    void DrawColliders()
    {
        var colliders = GetComponentsInChildren<Collider2D>();

        foreach (var collider in colliders)
            DrawCollider(collider);
    }
    
    void DrawCollider(Collider2D collider)
    {
        switch (collider)
        {
            case BoxCollider2D box:
                DrawBox2D(box);
                break;

            case CircleCollider2D circle:
                DrawCircle2D(circle);
                break;

            case CapsuleCollider2D capsule:
                DrawCapsule2D(capsule);
                break;

            case CompositeCollider2D composite:
                DrawComposite2D(composite);
                break;
        }
    }

    void DrawBox2D(BoxCollider2D box)
    {
        Vector3 center      = box.transform.TransformPoint(box.offset);
        Vector3 size        = box.size * box.transform.lossyScale;

        Vector3 topRight    = center + new Vector3(size.x, size.y) * 0.5f;
        Vector3 topLeft     = center + new Vector3(-size.x, size.y) * 0.5f;
        Vector3 bottomLeft  = center + new Vector3(-size.x, -size.y) * 0.5f;
        Vector3 bottomRight = center + new Vector3(size.x, -size.y) * 0.5f;

        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
    }

    void DrawCircle2D(CircleCollider2D circle)
    {
        Vector3 center      = circle.transform.TransformPoint(circle.offset);
        float radius        = circle.radius * Mathf.Max(circle.transform.lossyScale.x, circle.transform.lossyScale.y);

        int segments        = 32;
        float angle         = 0f;
        Vector3 lastPoint   = center + new Vector3(radius, 0);

        for (int i = 1; i <= segments; i++)
        {
            angle       = i / (float)segments * 360f * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint   = newPoint;
        }
    }

    void DrawCapsule2D(CapsuleCollider2D capsule)
    {
        Vector3 center  = capsule.transform.TransformPoint(capsule.offset);
        Vector2 size    = capsule.size * capsule.transform.lossyScale;
        
        float radius    = Mathf.Min(size.x, size.y) * 0.5f;

        Gizmos.DrawWireSphere(center + Vector3.up * (size.y * 0.5f - radius), radius);
        Gizmos.DrawWireSphere(center + Vector3.down * (size.y * 0.5f - radius), radius);
    }

    void DrawComposite2D(CompositeCollider2D composite)
    {
        for (int i = 0; i < composite.pathCount; i++)
        {
            Vector2[] path = new Vector2[composite.GetPathPointCount(i)];
            composite.GetPath(i, path);

            for (int j = 0; j < path.Length; j++)
            {
                Vector3 start = composite.transform.TransformPoint(path[j]);
                Vector3 end = composite.transform.TransformPoint(path[(j + 1) % path.Length]);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}


public enum HitboxBehavior
{
    Attached,
    Stationary,
    Projectile
}

public class HitboxDefinition : Definition
{
    public string Prefab                { get; init; }
    public Vector3 Offset               { get; init; }
    public Quaternion Quaternion        { get; init; }
    public int FrameStart               { get; init; }
    public int FrameEnd                 { get; init; }
    public bool AllowMultiHit           { get; init; }

    public HitboxBehavior Behavior      { get; init; }

    public float ProjectileSpeed        { get; init; }
    public Vector3 ProjectileDirection  { get; init; }
};

public class PendingHitbox
{
    public Actor Owner                  { get; init; }
    public WeaponAction Weapon          { get; init; }
    public HitboxDefinition Definition  { get; init; } 
}

public class HitboxInstance : Instance
{
    public int HitboxId                 { get; init; }
    public Actor Owner                  { get; init; }
    public WeaponAction Weapon          { get; set;  }
    public HitboxDefinition Definition  { get; init; }

    public int CurrentFrame             { get; set;  }
    public GameObject Hitbox            { get; set;  }
}

public struct HitEvent
{
    public int HitboxId                 { get; init; }
    public Actor Owner                  { get; set;  }
    public Actor Target                 { get; init; }
};





public class HitboxManager : RegisteredService, IServiceTick
{
    public static bool ShowDebugGizmos = true;


    readonly Dictionary<int, HitboxInstance> activeHitboxes     = new();
    readonly Queue<PendingHitbox> pending                       = new();

    readonly Queue<HitEvent> hitEvents                          = new();

    int hitboxIds;

    public override void Initialize()
    {
        EventBus<HitboxRequest>.Subscribe(HandleHitboxRequest);
    }


    public void Tick()
    {
        ProcessPending();
        ProcessHitEvents();
        ProcessActive();

        DebugLog();
    }


    void ProcessPending()
    {
        while(pending.TryDequeue(out var request))
            CreateHitbox(request);
    }

    void ProcessActive()
    {
        var toRemove = new List<int>();

        foreach (var (id, hitbox) in activeHitboxes)
        {
            hitbox.CurrentFrame++;

            if (hitbox.CurrentFrame == hitbox.Definition.FrameStart)
                ActivateHitbox(hitbox);

            if (hitbox.CurrentFrame == hitbox.Definition.FrameEnd)
            {
                DeactivateHitbox(hitbox);
                toRemove.Add(id);
            }
        }

        foreach (var id in toRemove)
            DestroyHitbox(id);
    }

    void ActivateHitbox(HitboxInstance hitbox)
    {
        Debug.Log($"Activating {hitbox.HitboxId} ");

        if (hitbox.Hitbox != null)
            hitbox.Hitbox.SetActive(true);
    }

    void DeactivateHitbox(HitboxInstance hitbox)
    {
        if (hitbox.Hitbox != null)
            hitbox.Hitbox.SetActive(false);
    }

    void DestroyHitbox(int hitboxId)
    {
        if (activeHitboxes.TryGetValue(hitboxId, out var instance))
        {
            if (instance.Hitbox != null)
                UnityEngine.Object.Destroy(instance.Hitbox);

            activeHitboxes.Remove(hitboxId);
        }
    }

    void ProcessHitEvents()
    {
        while (hitEvents.TryDequeue(out var hitEvent))
            ProcessHit(hitEvent);
    }

    // REWORK REQUIRED

    void ProcessHit(HitEvent hitEvent)
    {
        if (!activeHitboxes.TryGetValue(hitEvent.HitboxId, out var hitbox))
            return;

        if (!ShouldProcessHit(hitbox, hitEvent.Target))
            return;

        // EventBus<DamageEvent>.Raise(new DamageEvent
        // {
        // });
    }

    // REWORK REQUIRED

    bool ShouldProcessHit(HitboxInstance hitbox, Actor target)
    {
        if (hitbox.Owner == target)
            return false;

        return true;
    }

    void CreateHitbox(PendingHitbox pending)
    {

        Debug.Log("Creating");
        pending.Owner.Bridge.View.transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);

        Vector3 spawnPosition    = position + (rotation * pending.Definition.Offset);
        Quaternion spawnRotation = rotation * pending.Definition.Quaternion;

        var prefab      = Registry.Prefabs.Get(pending.Definition.Prefab);
        var hitbox      = UnityEngine.Object.Instantiate(prefab, spawnPosition, spawnRotation);
        
        var instance    = CreateInstance(pending);
        instance.Hitbox = hitbox;

        var controller  = hitbox.AddComponent<HitboxController>();
        controller.Configure(instance.HitboxId, instance.Owner, this);

        ApplyHitboxBehavior(hitbox, instance);

        activeHitboxes.Add(instance.HitboxId, instance);
    }

    HitboxInstance CreateInstance(PendingHitbox pending)
    {
        HitboxInstance instance = new()
        {
            HitboxId    = hitboxIds++,
            Owner       = pending.Owner,
            Definition  = pending.Definition
        };

        if (pending.Weapon != null)
            instance.Weapon = pending.Weapon;

        return instance;
    }


    void ApplyHitboxBehavior(GameObject hitbox, HitboxInstance instance)
    {
        switch (instance.Definition.Behavior)
        {
            case HitboxBehavior.Attached:
                hitbox.transform.SetParent(instance.Owner.Bridge.View.transform);
                break;

            case HitboxBehavior.Projectile:
                SetupProjectile(hitbox, instance);
                break;

            case HitboxBehavior.Stationary:
                break;
        }
    }

    // REWORK REQUIRED

    void SetupProjectile(GameObject hitbox, HitboxInstance instance)
    {
        // var projectile = hitbox.AddComponent<ProjectileMovement>();

        // instance.Owner.Bridge.View.transform.GetPositionAndRotation(out _, out Quaternion rotation);
        // Vector3 direction = rotation * instance.Definition.ProjectileDirection;

        // projectile.Initialize(instance.Definition.ProjectileSpeed, direction);
    }


    public void DetectHit(HitEvent hitEvent)
    {
        hitEvents.Enqueue(hitEvent);
    }

    void HandleHitboxRequest(HitboxRequest evt)
    {
        var owner       = evt.Payload.Owner;
        var definition  = evt.Payload.Definition;
        var weapon      = evt.Payload.Weapon;

        Debug.Log("Request");

        RequestHitbox(owner, definition, weapon);
    }

    void RequestHitbox(Actor owner, HitboxDefinition definition, WeaponAction weapon = null)
    {
        pending.Enqueue(new() { Owner = owner, Definition = definition, Weapon = weapon });
    }

    void DebugLog()
    {
        Log.Trace(LogSystem.Hitboxes, LogCategory.State, "Hitboxes", "Active",() => activeHitboxes.Count);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.HitboxManager;
}




public readonly struct HitboxRequestPayload
{
    public readonly Actor Owner                { get; init; }
    public readonly HitboxDefinition Definition { get; init; }
    public readonly WeaponAction Weapon         { get; init; }
}

public readonly struct HitboxRequest : IEvent
{
    public Guid Id                      { get; }
    public Request Action               { get; }
    public HitboxRequestPayload Payload { get; }

    public HitboxRequest(Guid id, Request action, HitboxRequestPayload payload)
    {
        Id      = id;
        Action  = action;
        Payload = payload;
    }
}
