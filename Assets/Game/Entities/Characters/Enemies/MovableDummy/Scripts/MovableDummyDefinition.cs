



[Definition]
public class MovableDummyDefinition : ActorDefinition
{
    public MovableDummyDefinition()
    {
        Name                        = nameof(MovableDummy);
        
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
        
        Lifecycle                   = new()
        {   
            Spawn                   = new()
            {
                Corpse              = true,
            },
            Respawn                 = new()
            {
                Enabled             = false,
            },
        };

        Animations                  = new()
        {   
            Spawn                   = new()
            {   
                Enabled             = true,
                Default             = "Spawn",
            },  

            Death                   = new()
            {   
                Enabled             = true,
                Default             = "Death",
            },
        };

        Rendering                   = new()
        {
            DepthSortingTier        = SortTier.Entity,
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
            MaxIntegrity            = 100,
        };

        Resource                    = new()
        {
            Integrity               = new()
            {
                AlertOnChange       = true,
            }
        };

        Corpse                      = new()
        {
            Name                    = nameof(DummyCorpse),
            FreshDuration           = 5,
            DecayDuration           = 5,
            ConsumeDuration         = 5,
            RemainsDuration         = 5,
        };

        Rendering                   = new()
        {
            DepthSortingTier        = SortTier.Ground,
        };
    }
}
