using character.context;


namespace character
{


public class Context
{
    public ContextCore     Core        { get; private set; }
    public CombatContext   Combat      { get; private set; }
    public MovementContext Movement    { get; private set; }

    public Context(ContextCore _core)
    {
        Core        = _core;
        Combat      = new CombatContext();
        Movement    = new MovementContext();
        
    }

    public void Deconstruct()
    {
        Movement.Deconstruct();
    }
}
}