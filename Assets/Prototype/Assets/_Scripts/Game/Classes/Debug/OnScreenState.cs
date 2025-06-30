using UnityEngine;
using state.character;

public class OnScreenState : MonoBehaviour
{
    [SerializeField] Character character;

    void OnGUI()
    {
        if (character.StateMachine.StateAttached.StateMachineMovement.ActiveState != null)
        {
            GUI.Label(new Rect(10, 10, 200, 20), $"State: {character.StateMachine.StateAttached.StateMachineMovement.ActiveState.GetType().Name}");
        }
    }
}

