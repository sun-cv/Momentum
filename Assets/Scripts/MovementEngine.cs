using System.Collections.Generic;
using System.Linq;
using UnityEngine;





public class MovementEngine : RegisteredService, IServiceTick
{
    EffectCache cache;

    Hero        hero;
    Context     context;
    Rigidbody2D body;

    readonly float maxSpeed     = Config.MOVEMENT_MAX_SPEED;
    readonly float acceleration = Config.MOVEMENT_ACCELERATION;
    readonly float friction     = Config.MOVEMENT_FRICTION;

    float modifier              = 1.0f;
    Dictionary<EffectType, List<IMovementModifier>> modifiers;

    Vector2 momentum;
    Vector2 velocity;

    public override void Initialize()
    {
        cache = new((effectInstance) => effectInstance.Effect is IType instance && (instance.Type == EffectType.Speed || instance.Type == EffectType.Grip));

        cache.OnApply   += CreateModifier;
        cache.OnCancel  += ClearModifier;
        cache.OnClear   += ClearModifier;
        
        modifiers = new();
    }

    public void Tick()   
    {
        ApplyFriction();

        ResolveEffectModifier();
        CalculateVelocity();

        if (context.CanMove)
            ApplyVelocity();

        DebugLog();
    }


    void ResolveEffectModifier()
    {
        if (modifiers.Count == 0)
            return;

        float sumOfTypeAverages = 0f;
        int typeCount = 0;

        foreach (var modifierList in modifiers.Values)
        {
            if (modifierList.Count == 0)
                continue;

            float typeSum = 0f;

            foreach (var modifier in modifierList)
                typeSum += modifier.Resolve();

            sumOfTypeAverages += typeSum / modifierList.Count;
            typeCount++;
        }

        modifier = typeCount > 0 ? sumOfTypeAverages / typeCount : 1f;
    }

    void CalculateVelocity()
    {
        float effectiveSpeed = Mathf.Clamp(hero.Speed * modifier, 0, maxSpeed);
        Vector2 targetVelocity = effectiveSpeed * context.MovementDirection.normalized;
        velocity = Vector2.MoveTowards(velocity, targetVelocity, acceleration * Clock.DeltaTime);
        
        momentum = velocity;
    }

    void ApplyFriction()
    {
        float frictionAmount = Mathf.Clamp01(friction * Clock.DeltaTime);
        velocity *= 1 - frictionAmount;
    }

    void ApplyVelocity() => body.linearVelocity = velocity;

    void DebugLog()
    {
        Logwin.Log("Movement Engine Velocity:", velocity);
        Logwin.Log("Movement Engine Modifier:", modifier);
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

    public void AssignHero(HeroController controller)
    {
        hero       = controller.Hero;
        context    = controller.Context;
        body       = controller.body;

        body.freezeRotation = true;
        body.gravityScale   = 0;
    }

    public Vector2 Velocity => velocity;
    public Vector2 Momentum => momentum;

    public UpdatePriority Priority => ServiceUpdatePriority.MovementEngine;
}


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
    readonly DurationCountdown timer;

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

