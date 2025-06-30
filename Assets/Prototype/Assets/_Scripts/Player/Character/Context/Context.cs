


public class CharacterContext
{

    public CharacterContextCore     Core        { get; private set; }
    public CharacterContextCombat   Combat      { get; private set; }
    public CharacterContextMovement Movement    { get; private set; }

    public CharacterContext(CharacterContextCore _core)
    {
        Core        = _core;
        Combat      = new CharacterContextCombat();
        Movement    = new CharacterContextMovement();
        
    }


    public void Deconstruct()
    {
        Movement.Deconstruct();
    }
}