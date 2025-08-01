using System;
using System.Collections.Generic;
using Momentum.Debugger;
using Momentum.Markers;
using UnityEngine;

// BUG WARNING - potential bug auto switching back to transmissionmode.automatic on oncomplete?

namespace Momentum.State
{

    public enum TransitionMode
    {
        automatic,
        command,
        disruption,
        forced
    }

    public class StateMachine
    {

        IState previous;
        IState current;

        TransitionMode mode                 = TransitionMode.automatic;

        Dictionary<Type, IState> states     = new();
        HashSet<Transition> transitions     = new();

        protected Dictionary<Type, StatusFlag> stateFlags   = new();

        protected Action<Type> OnStateEnter;
        protected Action<Type> OnStateExit;
        protected Action<Type> OnStateCancel;
        protected Action<Type> OnStateInterrupt;

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

        protected void SetState(IState state)
        {
            current = states[state.GetType()];
            EnterState(current);
        }
    
        public void ForceState<T>(Action onComplete) where T : IState
        {
            var state = states.GetValueOrDefault(typeof(T));

            SetCurrent(state);
            SetCurrentOncomplete(onComplete, TransitionMode.forced);

            CancelState(previous);
            EnterState(current);
        }


        public void InterruptState<T>(Action onComplete) where T : IState
        {
            if (current is not IInterruptible )
            {
                Debug.Log($"Failed to interrupt. {current.GetType()} is not interruptible");
                return;
            }

            if (states.GetValueOrDefault(typeof(T)) is not IStateDisruption state)
            {
                return;
            }

            SetCurrent(state);
            SetCurrentOncomplete(onComplete, TransitionMode.disruption);

            InterruptState(previous);
            EnterState(current);
        }


        public void CommandState<T>(Action onComplete) where T : IState
        {
            var command = states.GetValueOrDefault(typeof(T));

            if (command is not IStateCommand state)
            {
                Debug.Log("Attempted to command invalid State change, state required to be ICommand");
                return;
            }

            SetCurrent(state);
            SetCurrentOncomplete(onComplete, TransitionMode.command);

            ExitState(previous);
            EnterState(current);
        }


        protected void ChangeState(IState state)
        {
            if (state == current)
            {
                return;
            }
            
            SetCurrent(state);

            ExitState(previous);
            EnterState(current);
        }

        void EnterState(IState state)
        {
            state.Enter();
            SetStateFlag(state.GetType());
        }

        void ExitState(IState state)
        {
            state.Exit();
            ClearStateFlag(state.GetType());
        }

        void CancelState(IState state)
        {
            state.Cancel();
            ClearStateFlag(state.GetType());
        }

        void InterruptState(IState state)
        {
            state.Interrupt();
            ClearStateFlag(state.GetType());
        } 

        protected void SetStateFlag(Type type)
        {
            if (stateFlags.TryGetValue(type, out var flag)) flag.Set();
        }

        protected void ClearStateFlag(Type type)
        {
            if (stateFlags.TryGetValue(type, out var flag)) flag.Clear();
        }

        void SetCurrent(IState state)
        {
            previous = current;
            current  = states[state.GetType()];
        }

        void SetCurrentOncomplete(Action callback, TransitionMode _mode)
        {
            current.SetOnComplete(() => { callback(); mode = TransitionMode.automatic; });
            mode = _mode;
        }

        void CheckAutomaticTransition()
        {
            if (mode != TransitionMode.automatic)
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

        public void AddAnyTransition<T>(IState to, T condition)
        {
            transitions.Add(new Transition<T>(GetOrAddState(to), condition));
        }

        protected IState GetOrAddState(IState _state)
        {
            var state = states.GetValueOrDefault(_state.GetType());

            if (state == null)
            {
                state = _state;
                states.Add(state.GetType(), state);
            }

            return state;
        }


        public void DebugLogState(IState state)
        {
            StateDebugDisplay.SetState("State:", state.GetType().Name);
            StateDebugDisplay.SetLatch(mode);
        }

        protected void Any<T>(IState to, Func<bool> condition) => AddAnyTransition(to, condition);
        protected void Add(IState state, StatusFlag flag) { GetOrAddState(state); stateFlags.Add(state.GetType(), flag); }

    }   
}
