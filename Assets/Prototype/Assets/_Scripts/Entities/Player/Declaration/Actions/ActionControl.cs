

public class ActionControl
{
    protected FlagStatus    active   = new();

    public virtual void Initialize()            {}

    public virtual void Start()
    {
        active.Set();
    }

    public virtual void Stop()
    {
        active.Clear();
    }
    
    public virtual void Tick()                  {}
    public virtual void TickFixed()             {}    
}