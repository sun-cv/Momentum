using UnityEngine;





public class HeroState : State
{
    Hero owner;
    IntentSystem intent;

    EffectRegister effects;
    MovementEngine movement;

    TimePredicate idle;


    bool inactive       = false;

    bool stunned        = false;
    bool invulnerable   = false;

    public bool Inactive                            { get => inactive;          set => inactive    = value; }

    public bool Parrying                            => effects.Has<ShieldParryWindow>((effect) => effect is not null);
    public bool Blocking                            => effects.Has<ShieldBlockWindow>((effect) => effect is not null);

    public bool Disabled                            => Inactive || Stunned; // || other cc 
    public bool Stunned                             { get => effects.Has<IStunned>(effect => effect.Stunned,        defaultValue: stunned); set => stunned = value; }
    
    public bool Invulnerable                        { get => invulnerable;      set => invulnerable     = value; }

    public bool CanMove                             => effects.Can<IDisableMove>  (effect => effect.DisableMove,   defaultValue: !Disabled); 
    public bool CanAttack                           => effects.Can<IDisableAttack>(effect => effect.DisableAttack, defaultValue: !Disabled); 
    public bool CanRotate                           => effects.Can<IDisableRotate>(effect => effect.DisableRotate, defaultValue: !Disabled); 
   
    public Vector2 Facing                           => intent.Input.Facing;
    public Vector2 Direction                        => intent.Input.Direction;
    public Vector2 AimDirection                     => intent.Input.AimDirection;
    public Vector2 LastDirection                    => intent.Input.LastDirection;
    public Cardinal CardinalFacing                  => intent.Input.CardinalFacing;
    public Intercardinal IntercardinalAimDirection  => intent.Input.IntercardinalAimDirection;
    
    public Vector2 Velocity                         => movement.Velocity;
    public Vector2 Momentum                         => movement.Momentum;

    public bool IsMoving                            => Velocity != Vector2.zero;
    public TimePredicate IsIdle                     => idle ??= new (() => !IsMoving);

    public HeroState(Hero hero) : base(hero)
    {
        owner       = hero;
        effects     = hero.Effects;
        movement    = hero.Movement;
        intent      = hero.Intent;

    }
}