using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class HitboxManager : RegisteredService, IServiceTick, IInitialize
{
    public static bool ShowDebugGizmos = Settings.Debug.SHOW_HITBOXES;

    // -----------------------------------

    readonly List<(Request, HitboxAPI)> queue                   = new();
    readonly Dictionary<Guid, HitboxInstance> activeHitboxes    = new();
    readonly List<PendingHit> pendingHits                       = new();


    // ============================================================================

    public void Initialize()
    {
        Link.Global<Message<Request, HitboxAPI>>(HandleHitboxAPI);
        Link.Global<HitboxDirectionUpdate>(HandleDirectionUpdate);
    }

    // ===============================================================================

    public void Tick()
    {
        ProcessQueued();
        ProcessActive();
        ProcessHits();

        DebugLog();
    }

    // ===============================================================================

    void ProcessQueued()
    {
        foreach (var (request, pending) in queue)
        {
            ProcessHitbox(request, pending);
        }

        queue.Clear();
    }

    void ProcessHitbox(Request request, HitboxAPI instance)
    {
        switch (request)
        {
            case Request.Create:  CreateHitbox(instance);           break;
            case Request.Destroy: ProcessDestroyRequest(instance);  break;
        }
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

    void ActivateHitbox(HitboxInstance instance)
    {
        if (instance.Hitbox != null)
            instance.Hitbox.SetActive(true);
    }


    bool CheckConditionalHitbox(HitboxInstance instance)
    {
        return instance.Definition.Lifetime.ConditionalRelease(instance.Owner);
    }

    void DeactivateHitbox(HitboxInstance instance)
    {
        instance.Hitbox.SetActive(false);
    }

    void ProcessHits()
    {
        var grouped = pendingHits.GroupBy(hitbox => (hitbox.Attacker.Owner, hitbox.Target));

        foreach (var group in grouped)
        {
            var deflect = group.FirstOrDefault(hitbox => hitbox.Defender != null);
            var body    = group.FirstOrDefault(hitbox => hitbox.Defender == null);

            if (deflect != null)
            {
                SendDamagePackage(deflect.Attacker, deflect.Target, HitboxResult.Blocked);
                SendOtherPackages(deflect.Attacker, deflect.Target);

            }
            else if (body != null)
            {
                SendDamagePackage(body.Attacker, body.Target, HitboxResult.Hit);
                SendOtherPackages(body.Attacker, body.Target);

            }
        }

        pendingHits.Clear();
    }

    // ===================================
    //  Construction
    // ===================================

    void CreateHitbox(HitboxAPI pending)
    {
        pending.owner.Bridge.View.transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);

        Vector2 intentVector = GetSpawnDirection(pending);
        Quaternion intentRotation = Orientation.ToRotation(intentVector);
        Vector3 spawnPosition = position + (intentRotation * pending.definition.Form.Offset);

        var prefab = Assets.Get(pending.definition.Form.Prefab);
        var hitbox = UnityEngine.Object.Instantiate(prefab, spawnPosition, intentRotation);

        var instance = CreateInstance(pending);
        instance.Hitbox = hitbox;
        instance.SpawnDirection = intentVector;

        var controller = hitbox.AddComponent<HitboxController>();
        controller.Bind(this, instance.RuntimeId);

        ApplyHitboxBehavior(hitbox, instance);

        activeHitboxes.Add(instance.RuntimeId, instance);

        pending.hitboxId = instance.RuntimeId;

        PublishHitbox(pending);
    }

    HitboxInstance CreateInstance(HitboxAPI pending)
    {
        HitboxInstance instance = new()
        {
            Owner = pending.owner,
            Definition = pending.definition,
            Packages = pending.packages,
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

    void ProcessDestroyRequest(HitboxAPI instance)
    {
        var hitboxId = instance.hitboxId;

        if (ShouldProcessDestructionRequest(hitboxId))
            DestroyHitbox(hitboxId);
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

    public void OnHitDetected(Guid attackerId, Actor target, CollisionPhase phase)
    {
        if (!activeHitboxes.TryGetValue(attackerId, out var attacker)) 
            return;

        if (!ShouldHit(attacker, target, phase)) 
            return;

        RecordHit(attacker, target);
        pendingHits.Add(new PendingHit(attacker, target, null));
    }

    public void OnHitDetected(Guid attackerId, Actor target, CollisionPhase phase, Guid defenderId)
    {
        if (!activeHitboxes.TryGetValue(attackerId, out var attacker)) 
            return;

        if (!activeHitboxes.TryGetValue(defenderId, out var defender)) 
            return;

        if (!ShouldHit(attacker, target, phase)) 
            return;

        RecordHit(attacker, target);
        pendingHits.Add(new PendingHit(attacker, target, defender));
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

    void SendOtherPackages(HitboxInstance instance, Actor target)
    {
        foreach (var package in instance.Packages)
        {
            if (package is DamagePackage)
                continue;

            SendPackage(instance.Owner, target, package);
        }
    }

    void SendDamagePackage(HitboxInstance instance, Actor target, HitboxResult result)
    {
        var package = (DamagePackage)instance.Packages.First(package => package is DamagePackage);
        var context = new DamageContext(instance.Owner, target, package);

        context.Package.Result.Blocked = result == HitboxResult.Blocked;
        context.Package.Result.Parried = result == HitboxResult.Parried;

        Emit.Global(new DamageEvent(context));
    }

    void SendPackage(Actor source, Actor target, object package)
    {
        Emit.Global(new TriggerEvent
        {
            Source = source,
            Target = target,
            Package = package
        });
    }


    // ============================================================================
    //  Events
    // ============================================================================

    void HandleHitboxAPI(Message<Request, HitboxAPI> message)
    {
        queue.Add((message.Action, message.Payload));
    }

    void PublishHitbox(HitboxAPI instance)
    {
        Emit.Global(instance.Id, Response.Success, instance);
    }

    void HandleDirectionUpdate(HitboxDirectionUpdate update)
    {
        if (!activeHitboxes.TryGetValue(update.HitboxId, out var instance))
            return;

        instance.Hitbox.transform.rotation = Quaternion.Euler(0f, 0f, update.AimAngle);
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

    Vector2 GetSpawnDirection(HitboxAPI pending)
    {
        return pending.definition.Direction.Type switch
        {
            HitboxDirectionSource.Input => pending.definition.Direction.Scope switch
            {
                HitboxDirectionScope.Cardinal => pending.definition.Direction.Input.Aim.Cardinal,
                HitboxDirectionScope.Intercardinal => pending.definition.Direction.Input.Aim.Intercardinal,
                _ => pending.definition.Direction.Input.Aim.Intercardinal
            },
            HitboxDirectionSource.Explicit => pending.definition.Direction.Explicit,
            HitboxDirectionSource.OwnerFacing => pending.owner is IOrientable instance ? instance.Facing : Vector2.right,
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

record PendingHit(HitboxInstance Attacker, Actor Target, HitboxInstance Defender);


public class HitboxDefinition : Definition
{
    public HitboxFormDefinition Form                { get; init; }
    public HitboxBehaviorDefinition Behavior        { get; init; }
    public HitboxDirectionDefinition Direction      { get; init; }
    public HitboxLifetimeDefinition Lifetime        { get; init; }
}

public class HitboxFormDefinition : Definition
{
    public string Prefab                            { get; init; }
    public Vector3 Offset                           { get; init; }
}

public class HitboxBehaviorDefinition : Definition
{
    public HitboxBehavior Type                      { get; init; }
    public HitboxRole Role                          { get; init; }

    public bool TrackAim                            { get; init; }
    public int AimLockPeriod                        { get; init; }

    public bool AllowMultiHit                       { get; init; }
    public float MultiHitInterval                   { get; init; }
    // Projectile               
    public float ProjectileSpeed                    { get; init; }
    public Vector3 ProjectileDirection              { get; init; }
}

public class HitboxDirectionDefinition : Definition
{
    public HitboxDirectionSource Type               { get; init; }
    public HitboxDirectionScope Scope               { get; init; }
    public Vector2 Explicit                         { get; set;  }
    public InputIntentSnapshot Input                { get; set;  }

    public HitboxTrackingConstraint Constraint      { get; init; }
}


public class HitboxLifetimeDefinition : Definition
{
    public HitboxLifetime Type                      { get; init; }
    public int FrameStart                           { get; init; }
    public int FrameEnd                             { get; init; }
    public bool PersistPastSource                   { get; init; }

    public WeaponPhase Phase                        { get; init; }

    public Func<Actor, bool> ConditionalRelease     { get; init; }
}

public class HitboxInstance : Instance
{
    public Actor Owner                              { get; init; }
    public GameObject Hitbox                        { get; set;  }
    public HitboxDefinition Definition              { get; init; }

    public int CurrentFrame                         { get; set;  }
    public List<object> Packages                    { get; init; }
    public Vector2 SpawnDirection                   { get; set;  }

    public HashSet<Actor> HitActors                 { get;       } = new();
    public Dictionary<Actor, float> NextHitTime     { get;       } = new();
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                  Enums                                                 
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum HitboxResult
{
    Hit,
    Blocked,
    Parried,    
}

public enum HitboxRole
{
    Attack,
    Shield,
    Parry,
}

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

public enum HitboxTrackingConstraint
{
    None,
    AdjacentOrdinal
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class HitboxAPI : Payload
{
    public Actor owner;
    public Guid hitboxId;
    public HitboxDefinition definition;
    public List<object> packages;

    public HitboxAPI(Actor owner, HitboxDefinition definition, List<object> packages)
    {
        this.owner = owner;
        this.definition = definition;
        this.packages = packages ?? new();
    }
}

public class HitboxDirectionUpdate : IMessage
{
    public Guid   HitboxId  { get; init; }
    public float  AimAngle  { get; init; }
}
