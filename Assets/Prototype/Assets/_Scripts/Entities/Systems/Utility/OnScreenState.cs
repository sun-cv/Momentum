using UnityEngine;

public class OnScreenState : MonoBehaviour
{
    [SerializeField] Character character;


    void OnGUI()
    {
        GUIStyle labelStyle = new(GUI.skin.label)
        {
            fontSize = 10
        };
        labelStyle.normal.textColor = Color.white;

        if (character.StateMachine.Movement.ActiveState != null)
        {
            GUI.Label(new Rect(10, 10, 100, 20), $"State: {character.StateMachine.Movement.ActiveState.GetType().Name}", labelStyle);
        }
    }
}

