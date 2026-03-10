



[Definition]
public class HeroDefinition : ActorDefinition
{
    public HeroDefinition()
    {
        Name                        = "Hero";
        
        Stats                       = new()
        {
            MaxHealth               = 100,
            MaxEnergy               = 100,
            Speed                   = 5,
            Strength                = 10,
            Impact                  = 10,
        };

        Resource                    = new()
        {
            Health                  = new()
            {
                AlertOnChange       = true,
            },
            Armor                   = new()
            {
                AlertOnChange       = true,
            },
            Shield                  = new()
            {
                AlertOnChange       = true,
            },
            Energy                  = new()
            {
                AlertOnChange       = true,
            }
        };

        Physics                     = new()
        {
            Mass                    = 20,
            Friction                = Settings.Physics.FRICTION,
            BleedRatio              = 0.2f,
            BleedThreshold          = 2000f,
            PushResistance          = 0f,
            MomentumThreshold       = 0f,
        };

        Presence                    = new()
        {
            CanBeSetAbsent          = true,
        };

        Lifecycle                   = new()
        {   
            Respawn                 = new()
            {
                Enabled             = false
            },

            Corpse                  = new()
            {
                Enabled             = true,
                Name                = "HeroCorpse",
                PersistDuration     = 5,
            },
        };

        Animations              = new()
        {
            Spawn               = new()
            {
                Enabled         = false,
                Default         = "Spawn"
            },

            Death               = new()
            {
                Enabled         = false,
                Default         = "Death"
            },
        };
    }
}