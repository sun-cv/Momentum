using System;
using System.Collections.Generic;
using UnityEngine;



public class HitboxManager : RegisteredService, IServiceTick, IInitialize
{
    public static bool ShowDebugGizmos                          = Settings.Debug.SHOW_HITBOXES;

        // -----------------------------------

    readonly Dictionary<Guid, HitboxInstance> activeHitboxes    = new();
    readonly List<PendingHitbox> queue                         = new();
    
    // ============================================================================

    public void Initialize()
    {
        Link.Global<Message<Request, HitboxDeclarationEvent>>(HandleHitboxCreateRequest);
        Link.Global<Message<Request, HitboxIdEvent >>(HandleHitboxDestroyRequest);
    }

    // ===============================================================================
    
    public void Tick()
    {
        ProcessQueued();
        ProcessActive();

        DebugLog();
    }

    // ===============================================================================

    void ProcessQueued()
    {
        foreach(var hitbox in queue)
            CreateHitbox(hitbox);

        queue.Clear();
    }

    void ProcessActive()
    {
        var toRemove = new List<Guid>();

        foreach (var (id, instance) in activeHitboxes)
        {
            UpdateHitbox(instance);

            if (ShouldRemove(instance))
            {
                toRemove.Add(id);
            }
        }

        RemoveHitboxes(toRemove);
    }


    void UpdateHitbox(HitboxInstance instance)
    {
        instance.CurrentFrame++;

        if (IsActivationFrame(instance))
            ActivateHitbox(instance);

        if (instance.Definition.Lifetime.Type == HitboxLifetime.Conditional)
            CheckConditionalHitbox(instance);

        if (IsDeactivationFrame(instance))
            DeactivateHitbox(instance);
    }

    void RemoveHitboxes(List<Guid> ids)
    {
        foreach (var id in ids)
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

    void DeactivateHitbox(HitboxInstance instance)
    {
        instance.Hitbox.SetActive(false);
    }

        // ===================================
        //  Construction
        // ===================================

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
        controller.Bind(this, instance.RuntimeID);

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
            Packages    = pending.Packages,
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

        // ===================================
        //  Destruction
        // ===================================

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

        // ===================================
        //  Hit Detection
        // ===================================

    public void OnHitDetected(Guid hitboxId, Actor target, CollisionPhase phase)
    {
        if (!activeHitboxes.TryGetValue(hitboxId, out var instance))
            return;

        if (!ShouldHit(instance, target, phase))
            return;

        RecordHit(instance, target);
        SendHitboxPackages(instance, target);
    }

    public void OnHitExited(Guid hitboxId, Actor target)
    {
        if (!activeHitboxes.TryGetValue(hitboxId, out var instance))
            return;

        instance.NextHitTime.Remove(target);
    }

    bool ShouldHit(HitboxInstance instance, Actor target, CollisionPhase phase)
    {
        if (target == instance.Owner) 
            return false;

        if (phase == CollisionPhase.Stay && !instance.Definition.Behavior.AllowMultiHit)
            return false;

        if (!instance.Definition.Behavior.AllowMultiHit)
            return !instance.HitActors.Contains(target);

        if (instance.NextHitTime.TryGetValue(target, out float nextTime))
            return Clock.Time >= nextTime;

        return true;
    }

    void RecordHit(HitboxInstance instance, Actor target)
    {
        if (!instance.Definition.Behavior.AllowMultiHit)
            instance.HitActors.Add(target);
        else
            instance.NextHitTime[target] = Clock.Time + instance.Definition.Behavior.MultiHitInterval;
    }

    void SendHitboxPackages(HitboxInstance instance, Actor target)
    {
        foreach (var package in instance.Packages)
        {
            SendPackage(instance.Owner, target, package);
        }
    }

    void SendPackage(Actor source, Actor target, object package)
    {
        Emit.Global(Request.Queue, new TriggerEvent
        {
            Source  = source,
            Target  = target,
            Package = package
        });
    }



    // ============================================================================
    //  Events
    // ============================================================================

    void HandleHitboxCreateRequest(Message<Request, HitboxDeclarationEvent> message)
    {
        var owner       = message.Payload.Owner;
        var definition  = message.Payload.Definition;
        var packages    = message.Payload.Packages;

        queue.Add(new() { RequestId = message.Id, Owner = owner, Definition = definition, Packages = packages });
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

    // ===============================================================================
    //  Predicates
    // ===============================================================================

    bool IsActivationFrame(HitboxInstance instance)
    {
        return instance.CurrentFrame == instance.Definition.Lifetime.FrameStart;
    }
    
    bool IsDeactivationFrame(HitboxInstance instance)
    {
        return instance.CurrentFrame == instance.Definition.Lifetime.FrameEnd;
    }
    
    bool ShouldRemove(HitboxInstance instance)
    {
        return !instance.Hitbox.activeSelf;
    }
    // ============================================================================
    // Helpers
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

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Hitboxes);

    void DebugLog()
    {
        Log.Trace("Active", () => activeHitboxes.Count);
    }

    public override void Dispose()
    {
        // NO OP;
    }

    public UpdatePriority Priority => ServiceUpdatePriority.HitboxManager;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

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
    public List<object> Packages                { get; init; }
}       

public class HitboxInstance : Instance      
{           
    public Actor Owner                          { get; init; }
    public GameObject Hitbox                    { get; set;  }
    public HitboxDefinition Definition          { get; init; }

    public int CurrentFrame                     { get; set;  }
    public List<object> Packages                { get; init; }

    public HashSet<Actor> HitActors             { get; } = new();
    public Dictionary<Actor, float> NextHitTime { get; } = new();
}       


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Enums                                                 
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

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


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct HitboxDeclarationEvent
{
    public readonly Actor Owner                 { get; init; }
    public readonly HitboxDefinition Definition { get; init; }
    public readonly List<object> Packages       { get; init; }

    public HitboxDeclarationEvent(Actor owner, HitboxDefinition definition, List<object> packages)
    {
        Owner       = owner;
        Definition  = definition;
        Packages    = packages ?? new();
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