using UnityEngine;


public class GameInitializer : MonoBehaviour
{

    [SerializeField] private InputHandler   inputhandler;
    [SerializeField] private Character      character;


    private void Start()
    {
       character.Initialize(); 
    }


    private void Update()
    {
        character.Tick();        
    }

    private void FixedUpdate()
    {
        character.TickFixed();
    }

}
