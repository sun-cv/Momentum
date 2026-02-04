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

public enum HitboxDirection
{
    Cardinal,
    Intercardinal,
}

public class HitboxDefinition : Definition
{
    public string Prefab                        { get; init; }
    public Vector3 Offset                       { get; init; }
    public int FrameStart                       { get; init; }
    public int FrameEnd                         { get; init; }
    public bool PersistPastSource               { get; init; }
    public bool AllowMultiHit                   { get; init; }
    public HitboxBehavior Behavior              { get; init; }
    public HitboxLifetime Lifetime              { get; init; }
    public HitboxDirection AvailableDirections  { get; init; }
    public float ProjectileSpeed                { get; init; }
    public Vector3 ProjectileDirection          { get; init; }

    public WeaponAction Activation              { get; init; }
    public WeaponPhase Phase                    { get; init; }



    public Func<Actor, bool> ConditionalRelease { get; init; }
}

public class PendingHitbox      
{       
    public Guid RequestId                       { get; init; }
    public Actor Owner                          { get; init; }
    public HitboxDefinition Definition          { get; init; }
    public InputIntentSnapshot Input            { get; init; }
}       

public class HitboxInstance : Instance      
{       
    public Guid HitboxId                        { get; init; }
    public Actor Owner                          { get; init; }
    public WeaponAction Weapon                  { get; set;  }
    public HitboxDefinition Definition          { get; init; }
    public InputIntentSnapshot Input            { get; init; }

    public int CurrentFrame                     { get; set;  }
    public GameObject Hitbox                    { get; set;  }
}       

public struct HitEvent      
{       
    public Guid HitboxId                        { get; init; }
    public Actor Owner                          { get; set;  }
    public Actor Target                         { get; init; }
};

// ============================================================================
// HITBOX MANAGER
// ============================================================================


public class HitboxManager : RegisteredService, IServiceTick
{
    readonly Logger Log = Logging.For(LogSystem.Hitboxes);

    public static bool ShowDebugGizmos                          = true;

    readonly Dictionary<Guid, HitboxInstance> activeHitboxes    = new();

    readonly Queue<PendingHitbox>   pending                     = new();
    readonly Queue<HitEvent>        hitEvents                   = new();

    public override void Initialize()
    {
        Link.Global<Message<Request, MHitboxDeclaration>>(HandleHitboxCreateRequest);
        Link.Global<Message<Request, MHitboxIdentifier >>(HandleHitboxDestroyRequest);
    }


    public void Tick()
    {
        ProcessPending();
        ProcessHitEvents();
        ProcessActive();

        DebugLog();
    }

// ============================================================================
// Main Process
// ====================;========================================================



    void ProcessPending()
    {
        while(pending.TryDequeue(out var request))
            CreateHitbox(request);
    }

    void ProcessActive()
    {
        var toRemove = new List<Guid>();

        foreach (var (id, hitbox) in activeHitboxes)
        {
            hitbox.CurrentFrame++;

            if (hitbox.CurrentFrame == hitbox.Definition.FrameStart)
                ActivateHitbox(hitbox);


            if (hitbox.Definition.Lifetime == HitboxLifetime.Conditional)
            {
                if (!CheckConditionalHitbox(hitbox))
                    continue;
            }

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
        if (hitbox.Hitbox != null)
            hitbox.Hitbox.SetActive(true);
    }

    bool CheckConditionalHitbox(HitboxInstance hitbox)
    {
        return hitbox.Definition.ConditionalRelease(hitbox.Owner);
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

        Vector2 intentVector        = GetIntentVector(pending);
        Quaternion intentRotation   = Orientation.ToRotation(intentVector);
        Vector3 spawnPosition       = position + (intentRotation * pending.Definition.Offset);

        var prefab      = Assets.Get(pending.Definition.Prefab);
        var hitbox      = UnityEngine.Object.Instantiate(prefab, spawnPosition, intentRotation);
        
        var instance    = CreateInstance(pending);
        instance.Hitbox = hitbox;

        var controller  = hitbox.AddComponent<HitboxController>();
        controller.Configure(instance.HitboxId, instance.Owner, this);

        ApplyHitboxBehavior(hitbox, instance);

        activeHitboxes.Add(instance.HitboxId, instance);

        PublishHitbox(pending.RequestId, instance.HitboxId);
    }

    HitboxInstance CreateInstance(PendingHitbox pending)
    {
        HitboxInstance instance = new()
        {
            HitboxId    = Guid.NewGuid(),
            Owner       = pending.Owner,
            Definition  = pending.Definition,
            Input       = pending.Input,
        };

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

        if (instance.Definition.PersistPastSource && instance.Definition.Lifetime == HitboxLifetime.FrameBased)
            return false;

        return true;
    }

// ============================================================================
// HIT HANDLERS    - REWORK REQUIRED
// ============================================================================

    void ProcessHitEvents()
    {
        while (hitEvents.TryDequeue(out var hitEvent))
            ProcessHit(hitEvent);
    }


    void ProcessHit(HitEvent hitEvent)
    {
        if (!activeHitboxes.TryGetValue(hitEvent.HitboxId, out var hitbox))
            return;

        if (!ShouldProcessHit(hitbox, hitEvent.Target))
            return;
    }


    bool ShouldProcessHit(HitboxInstance hitbox, Actor target)
    {
        if (hitbox.Owner == target)
            return false;

        return true;
    }

    public void DetectHit(HitEvent hitEvent)
    {
        hitEvents.Enqueue(hitEvent);
    }

    // ============================================================================
    // EVENT HANDLERS
    // ============================================================================


    void HandleHitboxCreateRequest(Message<Request, MHitboxDeclaration> message)
    {
        var owner       = message.Payload.Owner;
        var definition  = message.Payload.Definition;
        var input       = message.Payload.Input;

        pending.Enqueue(new() { Owner = owner, Definition = definition, RequestId = message.Id, Input = input});
    }

    void HandleHitboxDestroyRequest(Message<Request, MHitboxIdentifier> message)
    {
        var hitboxId = message.Payload.HitboxId;

        if (ShouldProcessDestructionRequest(hitboxId))
            DestroyHitbox(hitboxId);
    }

    void PublishHitbox(Guid requestId, Guid instanceId)
    {
        Emit.Global(requestId, Response.Success, new MHitboxIdentifier(instanceId));
    }

    // ============================================================================
    // HELPERS
    // ============================================================================

    Vector2 GetIntentVector(PendingHitbox pending)
    {
        return pending.Definition.AvailableDirections switch
        {
            HitboxDirection.Cardinal        => pending.Input.Aim.Cardinal,
            HitboxDirection.Intercardinal   => pending.Input.Aim.Intercardinal,
            _ => pending.Input.Aim.Intercardinal
        };
    }


    void DebugLog()
    {
        Log.Trace("Active", () => activeHitboxes.Count);
    }
    // void Link<T>(Action<T> handler) where T : IEvent    => EventBus<T>.Subscribe(handler);
    // void Emit<T>(T evt) where T : IEvent                => EventBus<T>.Raise(evt);

    public UpdatePriority Priority => ServiceUpdatePriority.HitboxManager;
}


public readonly struct MHitboxDeclaration
{
    public readonly Actor Owner                 { get; init; }
    public readonly HitboxDefinition Definition { get; init; }
    public readonly InputIntentSnapshot Input   { get; init; }

    public MHitboxDeclaration(Actor owner, HitboxDefinition definition, InputIntentSnapshot input)
    {
        Owner       = owner;
        Definition  = definition;
        Input       = input;
    }

}

public readonly struct MHitboxIdentifier
{
    public readonly Guid HitboxId               { get; init; }

    public MHitboxIdentifier(Guid hitboxId)
    {
        HitboxId = hitboxId;
    }
}