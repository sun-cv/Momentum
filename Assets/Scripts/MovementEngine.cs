using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;





public class MovementEngine : RegisteredService, IServiceTick
{
    float groundFriction = Config.MOVEMENT_GROUND_FRICTION;
    float maxSpeed       = Config.MOVEMENT_MAX_SPEED;
    float acceleration   = Config.MOVEMENT_ACCELERATION;
    float deceleration   = Config.MOVEMENT_DECELERATION;

    EffectCache cache;

    Hero        hero;
    Context     context;
    Rigidbody2D body;

    float modifier;
    float modifierBase = 1.0f;

    Vector2 momentum;
    Vector2 velocity;


    public override void Initialize()
    {
        cache = new((EffectInstance) => EffectInstance.Effect is IType instance && (instance.Type == "SPEED" || instance.Type == "GRIP"));
    }

    public void Tick()   
    {
        CalculateEffectModifier();

        Logwin.Log("Effects", Services.Get<EffectRegister>().Effects.Count);
        Debug.Log(Services.Get<EffectRegister>().Effects.FirstOrDefault()?.Effect.Name);


        Logwin.Log("Cache", cache.Effects.Count);
        

        Logwin.Log("Movement Engine Modifier", modifier);

        Logwin.Log("Movement Engine Direction X", context.MovementDirection.x);
        Logwin.Log("Movement Engine Direction Y", context.MovementDirection.y);

        Vector2 targetVelocity = Math.Clamp(hero.Speed * modifier, 0, maxSpeed) * context.MovementDirection.normalized;
        Logwin.Log("Movement Engine Target", targetVelocity);

        float rate  = context.MovementDirection != Vector2.zero ? acceleration : deceleration;
        Logwin.Log("Movement Engine Rate", rate);
        
        velocity    = Vector2.MoveTowards( velocity, targetVelocity, rate * Clock.DeltaTime);
        Logwin.Log("Movement Engine Velocity", velocity);
    
        body.linearVelocity = velocity;
    }
    

    void CalculateEffectModifier()
    {
        modifier = modifierBase;

        foreach ( var effectInstance in cache.Effects)
        {
            if (effectInstance.Effect is not IModifiable instance)
                continue;

            modifier *= instance.Modifier;
        }

    }


    public void AssignHero(Hero hero, Context context, Rigidbody2D body)
    {
        this.hero       = hero;
        this.context    = context;
        this.body       = body;

        this.body.freezeRotation = true;
        this.body.gravityScale   = 0;
    }

    public UpdatePriority Priority => ServiceUpdatePriority.MovementEngine;
}


