using System.Collections.Generic;
using Momentum.Markers;
using UnityEngine;


namespace Momentum.Debugger
{


    public static class StateDebugDisplay
    {
        private static readonly Dictionary<string, string> currentStates = new();
        private static StatusFlag status;


        public static void SetState(string id, string stateName)
        {
            currentStates[id] = stateName;
        }

        public static void SetLatch(StatusFlag latch)
        {
            status = latch;
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
                GUILayout.Label($"Latch: {status.Value}");
            }
            GUILayout.EndVertical();
        }
    }
}