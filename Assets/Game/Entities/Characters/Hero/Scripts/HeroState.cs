using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;





public class HeroState : State
{
    Hero            owner;
    IntentSystem    intent;

    EffectRegister  effects;
    MovementEngine  movement;

    TimePredicate   idle;

    bool inactive               = false;
    bool stunned                = false;
    bool invulnerable           = false;
    bool impervious             = false;

    bool hasLockedAim           = false;
    bool hasLockedFacing        = false;
    bool hasLockedDirection     = false;

    public bool Inactive                            { get => inactive;          set => inactive    = value; }

    public bool Parrying                            => effects.Has<ShieldParryWindow>((effect) => effect is not null);
    public bool Blocking                            => effects.Has<ShieldBlockWindow>((effect) => effect is not null);

    public bool Disabled                            => Inactive || Stunned; // || other cc 
    public bool Stunned                             { get => effects.Has<IStunned>(effect => effect.Stunned,        defaultValue: stunned); set => stunned = value; }
    
    public bool Invulnerable                        { get => invulnerable;      set => invulnerable     = value; }
    public bool Impervious                          { get => impervious;        set => impervious       = value; }

    public bool CanMove                             => effects.Can<IDisableMove>  (effect => effect.DisableMove,   defaultValue: !Disabled); 
    public bool CanAttack                           => effects.Can<IDisableAttack>(effect => effect.DisableAttack, defaultValue: !Disabled); 
    public bool CanRotate                           => effects.Can<IDisableRotate>(effect => effect.DisableRotate, defaultValue: !Disabled); 
   
    public Direction Aim                            => intent.Input.Aim;
    public Direction Facing                         => intent.Input.Facing;
    public Direction Direction                      => intent.Input.Direction;
    public Direction LastDirection                  => intent.Input.LastDirection;
    
    public Direction LockedAim                      { get { if (CanRotate) { hasLockedAim       = false; return intent.Input.Aim;       }; if (!hasLockedAim)       { cachedLockedAim       = intent.Input.Aim;       hasLockedAim       = true; }; return cachedLockedAim;       }}
    public Direction LockedFacing                   { get { if (CanRotate) { hasLockedFacing    = false; return intent.Input.Facing;    }; if (!hasLockedFacing)    { cachedLockedFacing    = intent.Input.Facing;    hasLockedFacing    = true; }; return cachedLockedFacing;    }}
    public Direction LockedDirection                { get { if (CanRotate) { hasLockedDirection = false; return intent.Input.Direction; }; if (!hasLockedDirection) { cachedLockedDirection = intent.Input.Direction; hasLockedDirection = true; }; return cachedLockedDirection; }}

    public Vector2 Velocity                         => movement.Velocity;
    public Vector2 Momentum                         => movement.Momentum;

    public bool IsMoving                            => Velocity != Vector2.zero;
    public TimePredicate IsIdle                     => idle ??= new (() => !IsMoving);


    //
    //  Cache values:
    //

    Direction cachedLockedAim;
    Direction cachedLockedFacing;
    Direction cachedLockedDirection;

    public HeroState(Hero hero) : base(hero)
    {
        owner       = hero;
        effects     = hero.Effects;
        movement    = hero.Movement;
        intent      = hero.Intent;
    }
}
