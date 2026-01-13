using UnityEngine;





public class HeroState : State
{
    Hero hero;
    WorldPosition worldPosition;


    EffectRegister effects;
    MovementEngine movement;

    TimePredicate idle;

    bool stunned        = false;
    bool disabled       = false;
    bool invulnerable   = false;

    public bool Parrying                        => effects.Has<ShieldParryWindow>((effect) => effect is not null);
    public bool Blocking                        => effects.Has<ShieldBlockWindow>((effect) => effect is not null);

    public bool Stunned                         { get => effects.Has<IStunned>(effect => effect.Stunned,        defaultValue: stunned); set => stunned = value; }
    public bool Disabled                        { get => disabled;          set => disabled         = value; }
    public bool Invulnerable                    { get => invulnerable;      set => invulnerable     = value; }

    public bool CanMove                         => effects.Can<IDisableMove>  (effect => effect.DisableMove,   defaultValue: !Disabled); 
    public bool CanAttack                       => effects.Can<IDisableAttack>(effect => effect.DisableAttack, defaultValue: !Disabled); 
    public bool CanRotate                       => effects.Can<IDisableRotate>(effect => effect.DisableRotate, defaultValue: !Disabled); 




    public CardinalDirection FacingDirection    => Direction.FromMovement(MovementDirection);
    public CardinalDirection IntentDirection    => Direction.FromIntentZone(worldPosition.MouseDirectionFrom(hero.Bridge.View.transform.position));
    public Vector2 MovementDirection            => Services.Get<InputRouter>().RemoteMovementDirection;
    public Vector2 Velocity                     => movement.Velocity;
    public Vector2 Momentum                     => movement.Momentum;

    public bool IsMoving                        => Velocity != Vector2.zero;
    public TimePredicate IsIdle                 => idle ??= new (() => !IsMoving);

    public HeroState(Hero hero) : base(hero)
    {
        this.hero   = hero;
        effects     = hero.Effects;
        movement    = hero.Movement;

        worldPosition = Services.Get<WorldPosition>();
    }
}