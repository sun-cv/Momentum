

public abstract class BaseState<Context>
{
    protected Context context;

    public virtual void Enter()                 {}
    public virtual void Exit()                  {}
    
    public virtual void Initialize()            {}

    public virtual void Tick()                  {}
    public virtual void TickFixed()             {}

    public virtual void Trigger()               {}
    public virtual void TriggerOnAudio()        {}
    public virtual void TriggerOnCancel()       {}
    public virtual void TriggerOnCollider()     {}
    public virtual void TriggerOnAnimation()    {}
    public virtual void TriggerOnStateChange()  {}

    public virtual void AdvanceState()          {}

}

    // public override void Enter()                 
    // {
    // }
    // public override void Exit()                  
    // {
    // }
    // public override void Initialize()            
    // {
    // }
    // public override void Tick()                  
    // {
    // }
    // public override void TickFixed()             
    // {
    // }
    // public override void Trigger()               
    // {
    // }
    // public override void TriggerOnAudio()        
    // {
    // }
    // public override void TriggerOnCancel()       
    // {
    // }
    // public override void TriggerOnCollider()     
    // {
    // }
    // public override void TriggerOnAnimation()    
    // {
    // }