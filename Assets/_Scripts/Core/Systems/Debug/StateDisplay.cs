using System.Collections.Generic;
using UnityEngine;


namespace Momentum
{


    public static class StateDebugDisplay
    {
        private static readonly Dictionary<string, string> currentStates = new();
        private static TransitionMode status;


        public static void SetState(string id, string stateName)
        {
            currentStates[id] = stateName;
        }

        public static void SetLatch(TransitionMode mode)
        {
            status = mode;
        }

        public static void RemoveState(string id)
        {
            currentStates.Remove(id);
        }

        public static void OnGUI()
        {
            GUILayout.BeginVertical("box");
            foreach (var kvp in currentStates)
            {
                GUILayout.Label($"State: {kvp.Value}");
                GUILayout.Label($"Blocked: {status}");
            }
            GUILayout.EndVertical();
        }
    }
}