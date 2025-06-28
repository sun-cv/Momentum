using UnityEngine;

public class Character : MonoBehaviour
{


    private CharacterStateMachine stateMachine;


    public void Initialize()
    {
        stateMachine = new CharacterStateMachine(this);
        stateMachine.Initialize(stateMachine.stateAttached);
    }


    public void Tick()
    {

    }


    public void TickFixed()
    {

    }


}
