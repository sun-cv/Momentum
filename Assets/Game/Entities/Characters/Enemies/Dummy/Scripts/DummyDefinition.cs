



[Definition]
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
                Persists            = true,
                PersistDuration     = 5,
            },
        };

        Animations              = new()
        {
            Spawn               = new()
            {
                Enabled         = true,
                Default         = "Spawn"
            },

            Death               = new()
            {
                Enabled         = true,
                Default         = "Death"
            },
        };
    }
}