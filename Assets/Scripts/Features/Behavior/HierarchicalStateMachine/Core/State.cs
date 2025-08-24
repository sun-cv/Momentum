using System.Collections.Generic;

namespace Momentum 
{
    
    public abstract class State 
    {
        public readonly StateMachine Machine;
        public readonly State Parent;

        public State ActiveChild;

        readonly List<IActivity> activities = new();
        public IReadOnlyList<IActivity> Activities => activities;
        
        public State(State parent = null) 
        {
            Parent  = parent;
        }
        
        public void Add(IActivity activity) { if (activity != null) activities.Add(activity); }
        
        protected virtual State GetInitialState()   => null; // Initial child to enter when this state starts (null = this is the leaf)
        protected virtual State GetTransition()     => null; // Target state to switch to this frame (null = stay in current state)
        
        protected virtual void OnEnter()                    { }
        protected virtual void OnExit()                     { }
        protected virtual void OnUpdate(float deltaTime)    { }

        internal void Enter() 
        {
            if (Parent != null) Parent.ActiveChild = this;

            OnEnter();

            State init = GetInitialState();

            init?.Enter();
        }
        
        internal void Exit() 
        {
            ActiveChild?.Exit();
            ActiveChild = null;
            OnExit();
        }

        internal void Update(float deltaTime) 
        {
            State transition = GetTransition();

            if (transition != null) 
            {
                Machine.Sequencer.RequestTransition(this, transition);
                return;
            }
            
            ActiveChild?.Update(deltaTime);
            OnUpdate(deltaTime);
        }
        
        // Returns the deepest currently-active descendant state (the leaf of the active path).
        public State Leaf() 
        {
            State state = this;

            while (state.ActiveChild != null)
            {
                state = state.ActiveChild;
            }

            return state;
        }
        
        // Yields this state and then each ancestor up to the root (self → parent → ... → root).
        public IEnumerable<State> PathToRoot() 
        {
            for (State state = this; state != null; state = state.Parent) yield return state;
        }
    }
}