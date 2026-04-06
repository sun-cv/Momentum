



[Definition]
public class MovableDummyDefinition : ActorDefinition
{
    public MovableDummyDefinition()
    {
        Name                        = nameof(MovableDummy);
        
        Stats                       = new()
        {
            Health                  = new()
            {
                AlertOnChange       = true,
            },
            MaxHealth               = 100,
            MaxEnergy               = 100,
            Speed                   = 5,
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
        
        Lifecycle                   = new()
        {   
            Spawn                   = new()
            {
                Corpse              = "Test"
            },
            Respawn                 = new()
            {
                Enabled             = false,
            },
        };

        Appearance                  = new()
        {
            DepthSortingTier        = SortTier.Entity,
            Animations              = new()
            {   
                Spawn               = new()
                {   
                    Enabled         = true,
                    Default         = "Spawn",
                },  

                Death               = new()
                {   
                    Enabled         = true,
                    Default         = "Death",
                },
            }
        };
    }
}

[Definition]
public class MovableDummyCorpseDefinition : ActorDefinition
{
    public MovableDummyCorpseDefinition()
    {
        Name                        = nameof(MovableDummyCorpse);
        
        Stats                       = new()
        {
            Integrity               = new()
            {
                AlertOnChange       = true,
            },
            MaxIntegrity            = 100,
        };

        Lifecycle                   = new()
        {
            Corpse                  = new()
            {
                FreshDuration       = 5,
                DecayDuration       = 5,
                ConsumeDuration     = 5,
                RemainsDuration     = 5,
            }
        };

        Appearance                  = new()
        {
            DepthSortingTier        = SortTier.Ground,
        };
    }
}
