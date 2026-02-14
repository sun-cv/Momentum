using System;
using System.Collections.Generic;
using UnityEngine;



public enum HitboxBehavior
{
    Attached,
    Stationary,
    Projectile
}

public enum HitboxLifetime
{
    FrameBased,
    Conditional,
    Permanent
}

public enum HitboxDirectionScope
{
    Cardinal,
    Intercardinal,
}

public enum HitboxDirectionSource
{
    Input,
    Explicit,
    OwnerFacing
}

public class HitboxDefinition : Definition
{
    public HitboxFormDefinition      Form       { get; init; }
    public HitboxBehaviorDefinition  Behavior   { get; init; }
    public HitboxDirectionDefinition Direction  { get; init; }
    public HitboxLifetimeDefinition  Lifetime   { get; init; }
}

public class HitboxDirectionDefinition : Definition
{
    public HitboxDirectionSource Type           { get; init; }
    public HitboxDirectionScope Scope           { get; init; }
    public Vector2 Explicit                     { get; set;  }
    public InputIntentSnapshot Input            { get; set;  }
}


public class HitboxFormDefinition : Definition
{
    public string Prefab                        { get; init; }
    public Vector3 Offset                       { get; init; }
}

public class HitboxBehaviorDefinition : Definition
{
    public HitboxBehavior Type                  { get; init; }

    public bool AllowMultiHit                   { get; init; }
    public float MultiHitInterval               { get; init; }
    // Projectile
    public float ProjectileSpeed                { get; init; }
    public Vector3 ProjectileDirection          { get; init; }
}

public class HitboxLifetimeDefinition : Definition
{
    public HitboxLifetime Type                  { get; init; }
    public int FrameStart                       { get; init; }
    public int FrameEnd                         { get; init; }
    public bool PersistPastSource               { get; init; }

    public WeaponPhase Phase                    { get; init; }

    public Func<Actor, bool> ConditionalRelease { get; init; } 
}



public class PendingHitbox      
{       
    public Guid RequestId                       { get; init; }
    public Actor Owner                          { get; init; }
    public HitboxDefinition Definition          { get; init; }
    public object Package                       { get; init; }
}       

public class HitboxInstance : Instance      
{           
    public Actor Owner                          { get; init; }
    public GameObject Hitbox                    { get; set;  }
    public HitboxDefinition Definition          { get; init; }

    public int CurrentFrame                     { get; set;  }
    public object Package                       { get; init; }
}       


// ============================================================================
// HITBOX MANAGER
// ============================================================================


public class HitboxManager : RegisteredService, IServiceTick, IInitialize
{
    readonly Logger Log = Logging.For(LogSystem.Hitboxes);

    public static bool ShowDebugGizmos                          = Settings.Debug.SHOW_HITBOXES;

    readonly Dictionary<Guid, HitboxInstance> activeHitboxes    = new();

    readonly Queue<PendingHitbox>   pending                     = new();

    public void Initialize()
    {
        Link.Global<Message<Request, HitboxDeclarationEvent>>(HandleHitboxCreateRequest);
        Link.Global<Message<Request, HitboxIdEvent >>(HandleHitboxDestroyRequest);
    }

    public void Tick()
    {
        ProcessPending();
        ProcessActive();

        DebugLog();
    }

// ============================================================================
// Main Process
// ============================================================================

    void ProcessPending()
    {
        while(pending.TryDequeue(out var request))
            CreateHitbox(request);
    }

    void ProcessActive()
    {
        var toRemove = new List<Guid>();

        foreach (var (id, instance) in activeHitboxes)
        {
            var hitbox = instance.Definition;

            instance.CurrentFrame++;

            if (instance.CurrentFrame == hitbox.Lifetime.FrameStart)
                ActivateHitbox(instance);


            if (hitbox.Lifetime.Type == HitboxLifetime.Conditional)
            {
                if (!CheckConditionalHitbox(instance))
                    continue;
            }

            if (instance.CurrentFrame == hitbox.Lifetime.FrameEnd)
            {
                DeactivateHitbox(instance);
                toRemove.Add(id);
            }
        }

        foreach (var id in toRemove)
            DestroyHitbox(id);
    }

    void ActivateHitbox(HitboxInstance hitbox)
    {
        if (hitbox.Hitbox != null)
            hitbox.Hitbox.SetActive(true);
    }

    bool CheckConditionalHitbox(HitboxInstance hitbox)
    {
        return hitbox.Definition.Lifetime.ConditionalRelease(hitbox.Owner);
    }

    void DeactivateHitbox(HitboxInstance hitbox)
    {
        if (hitbox.Hitbox != null)
            hitbox.Hitbox.SetActive(false);
    }

// ============================================================================
// CREATION LOGIC
// ============================================================================


    void CreateHitbox(PendingHitbox pending)
    {
        pending.Owner.Bridge.View.transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);

        Vector2 intentVector        = GetSpawnDirection(pending);
        Quaternion intentRotation   = Orientation.ToRotation(intentVector);
        Vector3 spawnPosition       = position + (intentRotation * pending.Definition.Form.Offset);

        var prefab      = Assets.Get(pending.Definition.Form.Prefab);
        var hitbox      = UnityEngine.Object.Instantiate(prefab, spawnPosition, intentRotation);
        
        var instance    = CreateInstance(pending);
        instance.Hitbox = hitbox;

        var controller  = hitbox.AddComponent<HitboxController>();
        controller.Bind(this, instance.Owner, instance.RuntimeID, instance.Definition, instance.Package);

        ApplyHitboxBehavior(hitbox, instance);

        activeHitboxes.Add(instance.RuntimeID, instance);

        PublishHitbox(pending.RequestId, instance.RuntimeID);
    }

    HitboxInstance CreateInstance(PendingHitbox pending)
    {
        HitboxInstance instance = new()
        {
            Owner       = pending.Owner,
            Definition  = pending.Definition,
            Package     = pending.Package,
        };

        return instance;
    }


    void ApplyHitboxBehavior(GameObject hitbox, HitboxInstance instance)
    {
        switch (instance.Definition.Behavior.Type)
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

// ============================================================================
// DESTRUCTION LOGIC
// ============================================================================


    void DestroyHitbox(Guid hitboxId)
    {
        if (activeHitboxes.TryGetValue(hitboxId, out var instance))
        {
            if (instance.Hitbox != null)
                UnityEngine.Object.Destroy(instance.Hitbox);

            activeHitboxes.Remove(hitboxId);
        }
    }


    bool ShouldProcessDestructionRequest(Guid hitboxId)
    {
        if (!activeHitboxes.TryGetValue(hitboxId, out var instance))
            return false;

        if (instance.Definition.Lifetime.PersistPastSource && instance.Definition.Lifetime.Type == HitboxLifetime.FrameBased)
            return false;

        return true;
    }


    // ============================================================================
    // EVENT HANDLERS
    // ============================================================================


    void HandleHitboxCreateRequest(Message<Request, HitboxDeclarationEvent> message)
    {
        var owner       = message.Payload.Owner;
        var definition  = message.Payload.Definition;
        var package     = message.Payload.Package;

        pending.Enqueue(new() { RequestId = message.Id, Owner = owner, Definition = definition, Package = package });
    }

    void HandleHitboxDestroyRequest(Message<Request, HitboxIdEvent> message)
    {
        var hitboxId = message.Payload.HitboxId;

        if (ShouldProcessDestructionRequest(hitboxId))
            DestroyHitbox(hitboxId);
    }

    void PublishHitbox(Guid requestId, Guid instanceId)
    {
        Emit.Global(requestId, Response.Success, new HitboxIdEvent(instanceId));
    }

    // ============================================================================
    // HELPERS
    // ============================================================================

    Vector2 GetSpawnDirection(PendingHitbox pending)
    {
        return pending.Definition.Direction.Type switch
        {
            HitboxDirectionSource.Input => pending.Definition.Direction.Scope switch
            {
                HitboxDirectionScope.Cardinal       => pending.Definition.Direction.Input.Aim.Cardinal,
                HitboxDirectionScope.Intercardinal  => pending.Definition.Direction.Input.Aim.Intercardinal,
                _ => pending.Definition.Direction.Input.Aim.Intercardinal
            },
            HitboxDirectionSource.Explicit          => pending.Definition.Direction.Explicit,
            HitboxDirectionSource.OwnerFacing       => pending.Owner is IOrientable instance ? instance.Facing : Vector2.right,
            _ => Vector2.right
        };
    }

    public override void Dispose()
    {
        // NO OP;
    }

    void DebugLog()
    {
        Log.Trace("Active", () => activeHitboxes.Count);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.HitboxManager;
}


public readonly struct HitboxDeclarationEvent
{
    public readonly Actor Owner                 { get; init; }
    public readonly HitboxDefinition Definition { get; init; }
    public readonly object Package              { get; init; }

    public HitboxDeclarationEvent(Actor owner, HitboxDefinition definition, object package)
    {
        Owner       = owner;
        Definition  = definition;
        Package     = package;
    }

}

public readonly struct HitboxIdEvent
{
    public readonly Guid HitboxId               { get; init; }

    public HitboxIdEvent(Guid hitboxId)
    {
        HitboxId = hitboxId;
    }
}