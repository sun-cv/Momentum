



[Definition]
public class DummyDefinition : ActorDefinition
{
    public DummyDefinition()
    {
        Name                        = "Dummy";
        
        Stats                       = new()
        {
            MaxHealth               = 100,
            MaxEnergy               = 100,
            Speed                   = 0,
        };

        Resource                    = new()
        {
            Health                  = new()
            {
                AlertOnChange       = true,
            },
        };

        Physics                     = new()
        {
            Mass                    = 1000,

        };

        Presence                    = new()
        {
            CanBeSetAbsent          = false,
        };

        Lifecycle                   = new()
        {   
            Respawn                 = new()
            {
                Enabled             = false,
            },
            Corpse                  = new()
            {
                Name                = "DummyCorpse",
                Enabled             = true,
                PersistDuration     = 5,
            },
        };

        Animations                  = new()
        {   
            Spawn                   = new()
            {   
                Enabled             = true,
                Default             = "Spawn"
            },  

            Death                   = new()
            {   
                Enabled             = true,
                Default             = "Dying"
            },
        };
    }
}