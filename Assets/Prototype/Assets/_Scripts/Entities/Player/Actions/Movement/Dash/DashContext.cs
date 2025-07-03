

namespace character.context
{


public class DashContext
{
        // Requests
    public FlagRequest          Request                 { get; private set; } = new FlagRequest();

        // Context  
    public FlagStatus           Active                  { get; private set; } = new FlagStatus();

    

    public DashContext()
    {
        Subscribe();
    }

        // Subscribers
    public void Subscribe()
    {

        InputHandler.Instance.OnDash            += OnDash;
    }
    public void Deconstruct()
    {
        InputHandler.Instance.OnDash            -= OnDash;
    }

    public void OnDash()
    {

        
        Request.Set();
    }
}
}
