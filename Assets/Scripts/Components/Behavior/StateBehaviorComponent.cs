

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Momentum
{

    public class StateBehaviorComponent : MonoBehaviour
    {

        public StateMachine machine;
        private string lastPath;

        public void Initialize(State state)
        {
            var builder = new StateMachineBuilder(state);
            machine     = builder.Build();
        }


        public void Tick()
        {
            machine.Tick(Time.deltaTime);
            DebugLogState();
        }


        void DebugLogState()
        {
            var path = string.Join(" > ", machine.Root.Leaf().PathToRoot().Reverse().Select(n => n.GetType().Name));

            if (path != lastPath) 
            {
                Logwin.Log($"{machine.Root.GetType().Name.Substring(4)} Machine", path, "State Machine");
                lastPath = path;
            }
        }


    }


}