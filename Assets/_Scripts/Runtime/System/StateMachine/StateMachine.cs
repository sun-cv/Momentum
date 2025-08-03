using System;
using System.Collections.Generic;

using UnityEngine;

// BUG WARNING - potential bug auto switching back to transmissionmode.automatic on oncomplete?

namespace Momentum
{

    public enum TransitionMode
    {
        Automatic,
        Manual,
        Interrupted,
        Cancelled
    }

    public class StateMachine
    {

        State previous;
        State current;

        TransitionMode mode                 = TransitionMode.Automatic;

        Dictionary<Type, State> states     = new();
        HashSet<Transition> transitions     = new();

        protected Dictionary<Type, StatusFlag> stateFlags   = new();

        public void Tick()
        {
            CheckAutomaticTransition();           
            current.Tick();
            DebugLogState(current);
        }

        public void TickFixed()
        {
            current.TickFixed();
        }

        protected void SetState(State state)
        {
            current = state;
            EnterCurrentState();
        }

        public void CommandState<T>(Action<Result> reportResult) where T : State
        {
            var state = GetState<T>();

            SetTransitionMode(TransitionMode.Manual);

            SetCurrent(state);
            SetCurrentReportResult(reportResult);

            ExitAutomaticState();
            EnterCurrentState();
        }

        State GetState<T>()
        {
            return states.GetValueOrDefault(typeof(T));
        }

        public void InterruptState<T>() where T : IDisruption
        {
            if (current is not IInterruptible )
            {
                Debug.Log($"Failed to interrupt. {current.GetType()} is not interruptible");
                return;
            }

            current.Interrupt();
        }

        public void CancelState()
        {
            current.Cancel();
        }

        protected void SetTransitionMode(TransitionMode mode)
        {
            this.mode = mode;
        }


        protected void ChangeState(State state)
        {
            if (state == current)
            {
                return;
            }

            SetCurrent(state);

            ExitAutomaticState();
            EnterCurrentState();
        }


        void EnterCurrentState()
        {
            SetStateFlag(current.GetType());
            current.Enter();
        }

        void ExitAutomaticState()
        {
            if (previous is IAutomatic)
            {
                previous.Exit();
            }   
            ClearStateFlag(previous.GetType());
        }

        void SetCurrent(State state)
        {
            previous = current;
            current  = states[state.GetType()];
        }

        void SetCurrentReportResult(Action<Result> resultReport)
        {
            current.BindResult((result, transition)  => { resultReport(result); SetTransitionMode(transition);});
        }

        protected void SetStateFlag(Type type)
        {
            if (stateFlags.TryGetValue(type, out var flag)) flag.Set();
        }

        protected void ClearStateFlag(Type type)
        {
            if (stateFlags.TryGetValue(type, out var flag)) flag.Clear();
        }


        void CheckAutomaticTransition()
        {
            if (mode != TransitionMode.Automatic)
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
            foreach (var transition in transitions)
            {
                if (transition.Evaluate())
                {
                    return transition;
                }
            }
            return null;
        }

        public void AddAnyTransition<T>(State to, T condition)
        {
            transitions.Add(new Transition<T>(GetOrAddState(to), condition));
        }

        protected State GetOrAddState(State _state)
        {
            var state = states.GetValueOrDefault(_state.GetType());

            if (state == null)
            {
                state = _state;
                states.Add(state.GetType(), state);
            }

            return state;
        }


        public void DebugLogState(State state)
        {
            StateDebugDisplay.SetState("State:", state.GetType().Name);
            StateDebugDisplay.SetLatch(mode);
        }

        protected void Any<T>(State to, Func<bool> condition) => AddAnyTransition(to, condition);
        protected void Add(State state, StatusFlag flag) { GetOrAddState(state); stateFlags.Add(state.GetType(), flag); }

    }   
}
