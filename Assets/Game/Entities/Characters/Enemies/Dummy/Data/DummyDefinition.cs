




public class DummyDefinition : ActorDefinition
{
    public DummyDefinition()
    {
        Name                        = "Dummy";
        
        Stats                       = new()
        {
            MaxHealth               = 100,
            MaxMana                 = 100,
            Speed                   = 0,
            Attack                  = 0,
            Mass                    = 1000,
        };

        Lifecycle                   = new()
        {
            
        };
    }
}