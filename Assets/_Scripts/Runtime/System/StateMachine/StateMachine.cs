using System;
using System.Collections.Generic;
using Momentum.Debugger;
using Momentum.Markers;
using UnityEngine;

namespace Momentum.State
{

    public class StateMachine
    {
        protected StateNode previous;
        protected StateNode next;
        protected StateNode current;

        protected StatusFlag CanTransition  = new();

        Dictionary<Type, StateNode> nodes   = new();
        HashSet<Transition> anyTransitions  = new();

        

        public void Tick()
        {
            ValidateAnyTransitions();

            current.State.Tick();
        }

        public void TickFixed()
        {
            current.State.TickFixed();
        }

        public void Add(IState state)
        {
            GetOrAddNode(state);
        }

        public void SetState(IState state)
        {
            current = nodes[state.GetType()];
            current.State?.Enter();

            DebugLogState(state);
        }

        public void ChangeStateCommand<T>(Action onComplete) where T : IState
        {
            var node = nodes.GetValueOrDefault(typeof(T));

            if (node.State is ICommandState commandState)
            {
                commandState.SetCallback(() => { onComplete.Invoke(); CanTransition.Set();});
            }
            else 
            {
                Debug.Log("Attempted to command invalid State change, state required to be ICommandState");
                return;
            }

            ChangeState(node.State);
            CanTransition.Clear();
        }

        public void ChangeState(IState state)
        {
            if (state == current.State)
            {
                return;
            }

            previous = current;
            next     = nodes[state.GetType()];

            previous.State.Exit();
            next    .State.Enter();

            current = next;

            DebugLogState(state);
        }


        public void ValidateAnyTransitions()
        {            
            if (!CanTransition)
            {
                return;
            }

            var transition = GetTransition();
            
            if (transition != null)
            {
                ChangeState(transition.To);    
            }
        }

        Transition GetTransition()
        {
            foreach (var transition in anyTransitions)
            {
                if (transition.Evaluate())
                {
                    return transition;
                }
            }
            return null;
        }

        public void AddTransition<T>(IState from, IState to, T condition)
        {
            GetOrAddNode(from).AddTransition<T>(GetOrAddNode(to).State, condition);
        }

        public void AddAnyTransition<T>(IState to, T condition)
        {
            anyTransitions.Add(new Transition<T>(GetOrAddNode(to).State, condition));
        }

        protected StateNode GetOrAddNode(IState state)
        {
            var node = nodes.GetValueOrDefault(state.GetType());

            if (node == null)
            {
                node = new StateNode(state);
                nodes.Add(state.GetType(), node);
            }

            return node;
        }

        protected class StateNode
        {
            public IState State { get; }
            public HashSet<Transition> Transitions { get; }

            public StateNode(IState state)
            {
                State       = state;
                Transitions = new HashSet<Transition>();
            }

            public void AddTransition<T>(IState to, T condition)
            {
                Transitions.Add(new Transition<T>(to, condition));
            }
        }
        
        public void DebugLogState(IState state)
        {
            StateDebugDisplay.SetState("State:", state.GetType().Name);
            StateDebugDisplay.SetLatch(CanTransition);
        }

        protected void Any<T>(IState to, Func<bool> condition) => AddAnyTransition(to, condition);


    }
}




            // foreach (var transition in current.Transitions)
            // {
            //     if (transition.Evaluate())
            //     {
            //         return transition;
            //     }
            // }