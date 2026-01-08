using UnityEngine;





public class Context
{
    MovementEngine      movement;
    EffectRegister      effects;

    InputRouter         router;

    TimePredicate idle;

    public Context(Hero hero)
    {
        movement    = hero.Movement;
        router      = Services.Get<InputRouter>();
    }

    public bool CanMove                 => effects.Get<IDisableMove>  (effect => !effect.DisableMove  ); 
    public bool CanAttack               => effects.Get<IDisableAttack>(effect => !effect.DisableAttack); 
    public bool CanRotate               => effects.Get<IDisableRotate>(effect => !effect.DisableRotate); 

    public bool IsDisabled;
    public bool IsStunned;
    public bool IsInvulnerable;

    public Vector2 MovementDirection    => router.MovementDirection;
    public Vector2 Velocity             => movement.Velocity;
    public Vector2 Momentum             => movement.Momentum;

    public bool IsMoving                => Velocity != Vector2.zero;
    public TimePredicate IsIdle         => idle ??= new (() => !IsMoving);
}



