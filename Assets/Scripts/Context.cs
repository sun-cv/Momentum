using UnityEngine;





public class Context : Instance
{
    EffectRegister effects;
    InputRouter    router;

    TimePredicate idle;


    public void Initialize()
    {
        effects = Services.Get<EffectRegister>();
        router  = Services.Get<InputRouter>();
    }

    public WeaponSet weaponSet          = new SwordAndShield();

    public bool CanMove                 => effects.Get<IDisableMove>  (effect => !effect.DisableMove  ); 
    public bool CanAttack               => effects.Get<IDisableAttack>(effect => !effect.DisableAttack); 
    public bool CanRotate               => effects.Get<IDisableRotate>(effect => !effect.DisableRotate); 

    public bool IsDisabled;
    public bool IsStunned;
    public bool IsInvulnerable;

    public Vector2 MovementDirection    => router.MovementDirection;
    public Vector2 Velocity             => Services.Get<MovementEngine>().Velocity;
    public Vector2 Momentum             => Services.Get<MovementEngine>().Momentum;

    public bool IsMoving                => Velocity != Vector2.zero;
    public TimePredicate IsIdle         => idle ??= new (() => !IsMoving);
}



