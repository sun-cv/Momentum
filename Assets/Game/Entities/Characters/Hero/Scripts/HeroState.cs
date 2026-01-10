using UnityEngine;

public class HeroState : State
{
    EffectRegister effects;
    MovementEngine movement;

    TimePredicate idle;

    bool stunned;
    bool disabled;
    bool invulnerable;

    public bool Stunned                 { get => effects.Has<IStunned>(effect => effect.Stunned,        defaultValue: stunned); set => stunned = value; }
    public bool Disabled                { get => disabled;          set => disabled         = value; }
    public bool Invulnerable            { get => invulnerable;      set => invulnerable     = value; }

    public bool CanMove                 => effects.Can<IDisableMove>  (effect => !effect.DisableMove,   defaultValue: !Disabled); 
    public bool CanAttack               => effects.Can<IDisableAttack>(effect => !effect.DisableAttack, defaultValue: !Disabled); 
    public bool CanRotate               => effects.Can<IDisableRotate>(effect => !effect.DisableRotate, defaultValue: !Disabled); 

    public Vector2 MovementDirection    => Services.Get<InputRouter>().MovementDirection;
    public Vector2 Velocity             => movement.Velocity;
    public Vector2 Momentum             => movement.Momentum;

    public bool IsMoving                => Velocity != Vector2.zero;
    public TimePredicate IsIdle         => idle ??= new (() => !IsMoving);

    public HeroState(Hero entity) : base(entity)
    {

        effects = entity.Effects;
        movement = entity.Movement;
    }
}