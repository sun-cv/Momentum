




public class HeroDefinition : ActorDefinition
{
    public HeroDefinition()
    {
        Name                        = "Hero";
        
        Stats                       = new()
        {
            MaxHealth               = 100,
            MaxMana                 = 100,
            Speed                   = 5,
            Attack                  = 15,
            Mass                    = 10,
        };

        Presence                    = new()
        {
            EnableAbsentState       = true,
        };

        Lifecycle                   = new()
        {   
            Respawn                 = new()
            {
                Enabled             = false
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