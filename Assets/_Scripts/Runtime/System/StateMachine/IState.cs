using System;
using UnityEngine;


namespace Momentum.State
{


public interface IState
{

    void Enter();                 
    void SignalComplete();
    
    void Exit();        
    void Cancel();
    void Interrupt();

    void Tick();
    void TickFixed();

    void Audio()       {}
    void Animation()   {}

    void SetOnComplete(Action action);
}


public enum DisruptionType { Interrupt, Knockback, Stun, Slow }

public interface IStateAutomatic    : IState { public new void SignalComplete() {} public new void SetOnComplete(Action action) {}}
public interface IStateCommand      : IState {}
public interface IStateDisruption   : IState { DisruptionType Type { get; } };

public interface IInterruptible {}
public interface IKnockBackable {}
public interface IStunnable     {}
public interface ISlowable      {}




}