



[Definition]
public class MovableDummyDefinition : ActorDefinition
{
    public MovableDummyDefinition()
    {
        Name                        = "MovableDummy";
        
        Stats                       = new()
        {
            MaxHealth               = 100,
            MaxMana                 = 100,
            Speed                   = 5,
            Attack                  = 0,
        };

        Physics                     = new()
        {
            Mass                    = 5f,
            BleedThreshold          = 0f,
            BleedRatio              = 0f,
            PushResistance          = 0f,
            MomentumThreshold       = 1f,
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
                Name                = "MovableDummyCorpse",
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