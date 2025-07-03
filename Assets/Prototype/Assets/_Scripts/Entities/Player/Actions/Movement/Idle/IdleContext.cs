using UnityEngine;


namespace character.context
{

public class IdleContext
{
        // Requests

        // Context  
    public FlagStatus           Active                  { get; private set; } = new FlagStatus();


    public IdleContext()
    {
        Subscribe();
    }

        // Subscribers
    public void Subscribe()
    {
    }

}
}