using System;


namespace Momentum
{


public abstract class HeroState : State
{
    protected readonly Hero                 hero;

    protected AnimationController           animator;

    protected HeroAttributes                attribute;
    protected HeroContext                   context;
    protected HeroContext.Input             input;
    protected HeroContext.State             state;
    protected HeroContext.Condition         condition;
    protected HeroContext.Movement          movement;

    protected MovementMode movementMode;
    protected MovementIntent movementIntent;

    protected Action<Result, TransitionMode> result;
    protected Progress progress;

    protected const float crossFadeDuration = 0.1f;

    protected HeroState(Hero hero)
    {
        this.hero           = hero;
        this.animator       = hero.animator;

        this.context        = hero.context;
        this.input          = context.input;
        this.state          = context.state;
        this.attribute      = context.attributes;
        this.condition      = context.condition;
        this.movement       = context.movement;
    }

    public override void ReportResult(Result result, TransitionMode mode)
    {
        this.result.Invoke(result, mode);
    }

}

}

