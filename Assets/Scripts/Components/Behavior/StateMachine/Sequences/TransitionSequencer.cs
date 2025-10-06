using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Momentum 
{
    public class TransitionSequencer 
    {
        public readonly StateMachine Machine;
        
        ISequence sequencer;                 // current phase (deactivate or activate)
        Action nextPhase;                    // switch structure between phases
        (State from, State to)? pending;     // coalesce a single pending request
        State lastFrom, lastTo;
        
        CancellationTokenSource cancellationTokenSource;

        bool UseSequential = false;          // set false to use parallel

        public TransitionSequencer(StateMachine machine) 
        {
            Machine = machine;
        }

        // Request a transition from one state to another
        public void RequestTransition(State from, State to) 
        {
            if (to == null || from == to) 
                return;

            if (sequencer != null)
            { 
                pending = (from, to); 
                return; 
            }

            BeginTransition(from, to);
        }

        static List<PhaseStep> GatherPhaseSteps(List<State> chain, bool deactivate) 
        {
            var steps = new List<PhaseStep>();

            for (int i = 0; i < chain.Count; i++) 
            {
                var state       = chain[i];
                var activities  = chain[i].Activities;

                for (int j = 0; j < activities.Count; j++)
                {
                    var activity = activities[j];
                    bool include = deactivate ? (activity.Mode == ActivityMode.Active) : (activity.Mode == ActivityMode.Inactive);

                    if (!include) continue;

                    Debug.Log($"[Phase {(deactivate?"Exit":"Enter")}] state={state.GetType().Name}, activity={activity.GetType().Name}, mode={activity.Mode}");

                    steps.Add(cancellationToken => deactivate ? activity.DeactivateAsync(cancellationToken) : activity.ActivateAsync(cancellationToken));
                }
            }
            return steps;
        }
        
        // States to exit: from → ... up to (but excluding) lowestCommonAncestor; bottom→up order.
        static List<State> StatesToExit(State from, State lowestCommonAncestor) 
        {
            var list = new List<State>();
            for (var state = from; state != null && state != lowestCommonAncestor; state = state.Parent) list.Add(state);
            return list;
        }
        
        // States to enter: path from 'to' up to (but excluding) lowestCommonAncestor; returned in enter order (top→down).
        static List<State> StatesToEnter(State to, State lowestCommonAncestor) 
        {
            var stack = new Stack<State>();
            for (var state = to; state != lowestCommonAncestor; state = state.Parent) stack.Push(state);
            return new List<State>(stack);
        }

        void BeginTransition(State from, State to) 
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            
            var lowestCommonAncestor = LowestCommonAncestor(from, to);
            var exitChain  = StatesToExit(from, lowestCommonAncestor);
            var enterChain = StatesToEnter(to,  lowestCommonAncestor);
            
            // 1. Deactivate the “old branch”
            var exitSteps  = GatherPhaseSteps(exitChain, deactivate: true);

            sequencer = UseSequential ? new SequentialPhase(exitSteps, cancellationTokenSource.Token) : new ParallelPhase(exitSteps, cancellationTokenSource.Token);
            sequencer.Start();
            nextPhase = () => 
            {
                // 2. ChangeState
                Machine.ChangeState(from, to);
                // 3. Activate the “new branch”
                var enterSteps = GatherPhaseSteps(enterChain, deactivate: false);
                // sequencer = new NoopPhase();
                sequencer = UseSequential ? new SequentialPhase(enterSteps, cancellationTokenSource.Token) : new ParallelPhase(enterSteps, cancellationTokenSource.Token);
                sequencer.Start();
            };
        }

        void EndTransition() 
        {
            sequencer = null;

            if (pending.HasValue) 
            {
                (State from, State to) = pending.Value;
                pending = null;
                BeginTransition(from, to);
            }
        }

        public void Tick(float deltaTime) 
        {
            if (sequencer != null) 
            {
                if (sequencer.Update()) 
                {
                    if (nextPhase != null) 
                    {
                        var next = nextPhase;
                        nextPhase = null;
                        next();
                    } else 
                    {
                        EndTransition();
                    }
                }
                return; // while transitioning, we don't run normal updates
            }
            Machine.InternalTick(deltaTime);
        }

        // Compute the Lowest Common Ancestor of two states.
        public static State LowestCommonAncestor(State alpha, State bravo) 
        {
            // Create a set of all parents of 'a'
            var parents = new HashSet<State>();
            for (var state = alpha; state != null; state = state.Parent) parents.Add(state);

            // Find the first parent of 'b' that is also a parent of 'a'
            for (var state = bravo; state != null; state = state.Parent)
                if (parents.Contains(state))
                    return state;

            // If no common ancestor found, return null
            return null;
        }
    }
}