using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class MovementModifierHandler
{

    readonly List<EffectType> acceptedModifiers                         = new() 
    { 
        EffectType.Speed,
        EffectType.Grip
    };

        // -----------------------------------

    readonly EffectCache cache;    

        // -----------------------------------

    readonly Dictionary<EffectType, List<IMovementModifier>> modifiers  = new();

        // -----------------------------------

    float modifier = 1.0f;

    // ===============================================================================

    public MovementModifierHandler(Actor actor)
    {
        cache = new(actor.Emit, (effectInstance) => effectInstance.Effect is IType instance && acceptedModifiers.Contains(instance.Type));

        cache.OnApply   += CreateModifier;
        cache.OnCancel  += ClearModifier;
        cache.OnClear   += ClearModifier;
    }

    // ===============================================================================

    public float Calculate()
    {
        float sumOfAverages = 0f;
        int typeCount       = 0;

        foreach (var list in modifiers.Values)
        {
            int count = list.Count;
            if (count == 0) continue;

            float sum = 0f;

            for (int modifier = 0; modifier < count; modifier++)
                sum += list[modifier].Resolve();

            sumOfAverages += sum / count;
            typeCount++;
        }

        modifier = typeCount == 0 ? 1f : sumOfAverages / typeCount;
        return modifier;
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
            var modifier = list.FirstOrDefault(modifier => modifier.Instance == instance);
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
            _ => null
        };
    }

    // ===============================================================================


    public float Modifier       => modifier;
    public EffectCache Cache    => cache;
}    


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public interface IMovementModifier
{
    public EffectInstance Instance { get; }
    public float Resolve();
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                       Behaviours
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public class SpeedMovementModifier : IMovementModifier
{
    readonly EffectInstance instance;

    // ===============================================================================

    public SpeedMovementModifier(EffectInstance instance)
    {
        this.instance = instance;
    }
    
    // ===============================================================================

    public float Resolve()
    {
        return ((IModifiable)instance.Effect).Modifier;
    }

    public EffectInstance Instance => instance;
}

public class GripMovementModifier : IMovementModifier
{
    readonly EffectInstance instance;
    readonly DualCountdown timer;

    // ===============================================================================

    public GripMovementModifier(EffectInstance effectInstance)
    {
        instance    = effectInstance;
        timer       = ((IModifiable)effectInstance.Effect).ModifyTimespan != -1 ? new(((IModifiable)effectInstance.Effect).ModifyTimespan) : new(((IModifiable)effectInstance.Effect).ModifyDurationFrames);
        timer.Start();
    } 

    // ===============================================================================

    public float Resolve()
    {
        var effect = instance.Effect as IModifiable;
        return Mathf.Lerp(effect.Modifier, effect.ModifyTarget, timer.PercentComplete);
    }

    public EffectInstance Instance => instance;
}
