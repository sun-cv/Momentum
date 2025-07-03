using UnityEngine;


namespace character.context
{

public class SprintContext
{
        // Requests

        // Context  
    public FlagStatus           Active                  { get; private set; } = new FlagStatus();


    public SprintContext()
    {
        Subscribe();
    }

        // Subscribers
    public void Subscribe()
    {
    }

}
}