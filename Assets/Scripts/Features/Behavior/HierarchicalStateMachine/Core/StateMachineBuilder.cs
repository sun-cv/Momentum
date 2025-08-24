using System.Collections.Generic;
using System.Reflection;

namespace Momentum 
{
    public class StateMachineBuilder 
    {
        readonly State root;
        
        public StateMachineBuilder(State root) 
        {
            this.root = root;
        }

        public StateMachine Build() 
        {
            var machine = new StateMachine(root);
            Wire(root, machine, new HashSet<State>());
            return machine;
        }

        void Wire(State state, StateMachine machine, HashSet<State> visited) 
        {
            if (state == null) return;
            if (!visited.Add(state)) return; // State is already wired
            
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var machineField = typeof(State).GetField("Machine", flags);
            machineField?.SetValue(state, machine);

            foreach (var fld in state.GetType().GetFields(flags)) {
                if (!typeof(State).IsAssignableFrom(fld.FieldType)) continue; // Only consider fields that are State
                if (fld.Name == "Parent") continue; // Skip back-edge to parent
                
                var child = (State)fld.GetValue(state);
                if (child == null) continue;
                if (!ReferenceEquals(child.Parent, state)) continue; // Ensure it's actually our direct child
                
                Wire(child, machine, visited); // Recurse into the child
            }
        }
    }
}