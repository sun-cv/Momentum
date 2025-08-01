using System;
using Momentum.State;


namespace Momentum.Actor.Hero
{


public abstract class BaseState : IState
{
    protected readonly Hero                 hero;

     protected AnimationController           animator;

    protected HeroAttributes                attribute;
    protected HeroContext                   context;
    protected HeroContext.Input             input;
    protected HeroContext.State             state;
    protected HeroContext.Condition         condition;
    protected HeroContext.Movement          movement;

    protected Action OnComplete;

    protected const float crossFadeDuration = 0.1f;

    protected BaseState(Hero hero)
    {
        this.hero       = hero;
        this.animator   = hero.animator;

        this.context    = hero.context;
        this.input      = context.input;
        this.state      = context.state;
        this.attribute  = context.attributes;
        this.condition  = context.condition;
        this.movement   = context.movement;
    }

    public virtual void Enter()
    {
        // Noop
    }

    public virtual void Exit()
    {
        // Noop
    }

    public virtual void Tick()
    {
        // Noop
    }

    public virtual void TickFixed()
    {
        // Noop
    }

    public virtual void SignalComplete()
    {
        // noop
    }

    public virtual void Cancel()
    {
        // noop
    }

    public virtual void Interrupt()
    {
        // noop
    }

    public virtual void SetOnComplete(Action action)
    {
        // noop
    }


}

}

