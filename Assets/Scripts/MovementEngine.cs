using System.Collections.Generic;
using System.Linq;
using UnityEngine;





public class MovementEngine : RegisteredService, IServiceTick
{
    readonly float maxSpeed     = Config.MOVEMENT_MAX_SPEED;
    readonly float acceleration = Config.MOVEMENT_ACCELERATION;
    readonly float friction     = Config.MOVEMENT_FRICTION;

    readonly bool normalizeVelocity = false;

    EffectCache cache;

    Hero        hero;
    Context     context;
    Rigidbody2D body;

    Weapon      weapon;
    WeaponPhase phase;

    float modifier              = 1.0f;
    Dictionary<EffectType, List<IMovementModifier>> modifiers;

    float speed;

    bool    lockDirection;

    Vector2 direction;
    Vector2 directionLock;
    Vector2 directionTrail;

    Vector2 momentum;
    Vector2 velocity;
    Vector2 subPixelAccumulator;


    public override void Initialize()
    {
        cache = new((effectInstance) => effectInstance.Effect is IType instance && (instance.Type == EffectType.Speed || instance.Type == EffectType.Grip));

        cache.OnApply   += CreateModifier;
        cache.OnCancel  += ClearModifier;
        cache.OnClear   += ClearModifier;
        
        modifiers = new();

        EventBus<WeaponPublish>.Subscribe(HandleWeaponPublish);
    }

    public void Tick()   
    {
        SetDirection();
        SetSpeed();

        ApplyFriction();
        CalculateModifier();
        CalculateVelocity();

        if (HasActiveWeapon())
            ApplyWeaponMovement();

        if (CanMove())
            ApplyVelocity();

        DebugLog();
    }

    bool CanMove()
    {
        return context.CanMove;
    }

    // ============================================================================
    // MOVEMENT CALCULATIONS
    // ============================================================================


    void CalculateVelocity()
    {

        Vector2 targetVelocity = Mathf.Clamp(speed * modifier, 0, maxSpeed) * direction;

        velocity = Vector2.MoveTowards(velocity, targetVelocity, acceleration * Clock.DeltaTime);
        momentum = velocity;
    }

    void CalculateModifier()
    {
        float sumOfAverages = 0f;
        int typeCount       = 0;
    
        foreach (var list in modifiers.Values)
        {
            int count = list.Count;
            if (count == 0) continue;
    
            float sum = 0f;

            for (int i = 0; i < count; i++)
                sum += list[i].Resolve();
    
            sumOfAverages += sum / count;
            typeCount++;
        }
    
        modifier = typeCount == 0 ? 1f : sumOfAverages / typeCount;
    }


    void ApplyFriction() => velocity *= 1 - Mathf.Clamp01(friction * Clock.DeltaTime);
    void ApplyVelocity() => body.linearVelocity = velocity;


    // ============================================================================
    // WEAPON MANAGEMENT
    // ============================================================================

    void HandleWeaponPublish(WeaponPublish evt)
    {
        switch(evt.Action)
        {
            case Publish.Equipped:
                HandleWeaponEquip(evt.Payload.Weapon);
                break;
            case Publish.PhaseChange:
                HandleWeaponPhaseChange(evt.Payload.Phase);
                break;
            case Publish.Released:
                HandleWeaponRelease();
                break;
        }
    }

    void HandleWeaponEquip(Weapon weapon)
    {
        this.weapon = weapon;

        if (weapon.LockDirection)
        {
            directionLock = direction == Vector2.zero ? directionTrail : direction;
            lockDirection = true;
        }

    }

    void HandleWeaponPhaseChange(WeaponPhase phase)
    {
        this.phase = phase;
    }

    void HandleWeaponRelease()
    {
        weapon          = null;
        lockDirection   = false;
    }

    void ApplyWeaponMovement()
    {
        if (!weapon.WeaponOverridesMovement)
            return;

        float weaponSpeed = weapon.Speed >= 0 ? weapon.Speed : speed;
        float weaponMod = weapon.Modifier >= 0 ? weapon.Modifier : modifier;

        Vector2 targetVelocity = Mathf.Clamp(weaponSpeed * weaponMod, 0, maxSpeed) * direction;

        velocity = targetVelocity;
        momentum = velocity;
    }


    // ============================================================================
    // EFFECT MODIFIER MANAGEMENT
    // ============================================================================


    void CreateModifier(EffectInstance instance)
    {
        var coded = instance.Effect as IType;

        if (!modifiers.ContainsKey(coded.Type))
            modifiers[coded.Type] = new List<IMovementModifier>();

        modifiers[coded.Type].Add(CreateModifierForType(coded.Type, instance));
    }


    void ClearModifier(EffectInstance instance)
    {
        var coded = instance.Effect as IType;

        if (modifiers.TryGetValue(coded.Type, out var list))
        {
            var modifier = list.FirstOrDefault(m => m.Instance == instance);
            if (modifier != null)
                list.Remove(modifier);
        }
    }

    IMovementModifier CreateModifierForType(EffectType type, EffectInstance instance)
    {
        return type switch
        {
            EffectType.Speed => new SpeedMovementModifier(instance),
            EffectType.Grip  => new GripMovementModifier(instance),
            _ => throw new System.ArgumentException($"Unsupported effect type: {type}")
        };
    }



    // ============================================================================
    // HELPER METHODS
    // ============================================================================

    bool HasActiveWeapon()   => weapon != null;

    void SetDirection()
    {
        if (context.MovementDirection == Vector2.zero && direction != Vector2.zero)
            directionTrail = direction;

        Vector2 inputDir = lockDirection ? directionLock : context.MovementDirection;

        if (normalizeVelocity && inputDir.sqrMagnitude > 1f)
            inputDir = inputDir.normalized;

        direction = inputDir;
    }

    void SetSpeed()
    {
        speed = hero.Speed;
    }

    void DebugLog()
    {
        Log.Debug(LogSystem.Movement, LogCategory.State, "Velocity", () => velocity);
        Log.Debug(LogSystem.Movement, LogCategory.State, "Modifier", () => modifier);
        Log.Trace(LogSystem.Movement, LogCategory.Validation, "Effect Cache", () => cache.Effects.Count);
        Log.Trace(LogSystem.Movement, LogCategory.State, "ActiveEffects", () => $"{string.Join(", ", cache.Effects.Select(effect => effect.Effect.Name))}");
    }


    public void AssignHero(Hero hero)
    {
        this.hero       = hero;
        this.context    = hero.Context;
        this.body       = hero.Character.body;

        body.freezeRotation = true;
        body.gravityScale   = 0;
        body.interpolation  = RigidbodyInterpolation2D.Interpolate;
    }

    public Vector2 Velocity => velocity;
    public Vector2 Momentum => momentum;

    public UpdatePriority Priority => ServiceUpdatePriority.MovementEngine;
}





// ============================================================================
// MODIFIERS
// ============================================================================


public interface IMovementModifier
{
    public EffectInstance Instance { get; }
    public float Resolve();
}


public class SpeedMovementModifier : IMovementModifier
{
    readonly EffectInstance instance;
    public SpeedMovementModifier(EffectInstance instance) => this.instance = instance; 

    public float Resolve()
    {
        return ((IModifiable)instance.Effect).Modifier;
    }
    public EffectInstance Instance => instance;
}

public class GripMovementModifier : IMovementModifier
{
    readonly EffectInstance instance;
    readonly ClockTimer timer;

    public GripMovementModifier(EffectInstance effectInstance)
    {
        instance    = effectInstance;
        timer       = new(((IModifiable)effectInstance.Effect).ModifierSpeed);
        timer.Start();
    } 

    public float Resolve()
    {
        var effect = instance.Effect as IModifiable;
        return Mathf.Lerp(effect.Modifier, effect.ModifierTarget, timer.PercentComplete);
    }

    public EffectInstance Instance => instance;
}

