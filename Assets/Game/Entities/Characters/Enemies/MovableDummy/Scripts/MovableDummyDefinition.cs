



[Definition]
public class MovableDummyDefinition : ActorDefinition
{
    public MovableDummyDefinition()
    {
        Name                        = "MovableDummy";
        
        Stats                       = new()
        {
            MaxHealth               = 100,
            MaxEnergy               = 100,
            Speed                   = 5,
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
            Mass                    = 20f,
            Friction                = Settings.Physics.FRICTION,
            BleedThreshold          = 0f,
            BleedRatio              = 0f,
            PushResistance          = 0f,
            MomentumThreshold       = 25f,
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