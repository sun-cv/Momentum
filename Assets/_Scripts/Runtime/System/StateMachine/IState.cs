using System;
using UnityEngine;


namespace Momentum.State
{


public interface IState
{

    void Enter();                 
    void Exit();        

    void Tick();
    void TickFixed();
    void Initialize()           {}

    void Trigger()              {}
    void TriggerOnAudio()       {}
    void TriggerOnCancel()      {}
    void TriggerOnCollider()    {}
    void TriggerOnAnimation()   {}
    void TriggerOnStateChange() {}
}

public interface ILocomotionState   : IState {}
public interface ICommandState      : IState { public void SetCallback(Action action); public void OnComplete(); }




}