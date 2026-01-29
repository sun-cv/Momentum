




public class Dummy : Actor, IDummyEntity, IDepthSorted, IDepthColliding
{
    
    //========================================
    // Properties
    //========================================

    public float MaxHealth                          { get; set; } = 1000;
    public float Health                             { get; set; }

    //========================================
    // State
    //========================================

    public bool Inactive                            { get; set; }
    public bool Disabled                            { get; set; }
    public bool Stunned                             { get; set; }
    public bool Invulnerable                        { get; set; } 


    public void Initialize()
    {
        Health  = MaxHealth;
    }

}


