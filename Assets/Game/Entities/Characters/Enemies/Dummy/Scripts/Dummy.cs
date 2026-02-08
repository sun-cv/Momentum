




public class Dummy : Agent, IDummy
{
    //========================================
    // Properties
    //========================================

    public float MaxHealth                          { get; set; } = 100;
    public float Health                             { get; set; } = 100;

    //========================================
    // State
    //========================================

    public bool Inactive                            { get; set; } = false;
    public bool Disabled                            { get; set; } = false;
    public bool Stunned                             { get; set; } = false;
    public bool Invulnerable                        { get; set; } = false;
    public bool Impervious                          { get; set; } = false;


    public void Initialize()
    {
        Health  = MaxHealth;
    }

}


