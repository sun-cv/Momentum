


[Definition]
public class DummyDefinition : ActorDefinition
{
    public DummyDefinition()
    {
        Name                        = nameof(Dummy);
        
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
            Spawn                   = new()
            {
                Corpse              = true
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
                Default             = "Spawn"
            },  

            Death                   = new()
            {   
                Enabled             = true,
                Default             = "Death"
            },
        };

        Rendering                   = new()
        {
            DepthSortingTier        = SortTier.Ground,
        };
    }
}

[Definition]
public class DummyCorpseDefinition : ActorDefinition
{
    public DummyCorpseDefinition()
    {
        Name                        = nameof(Dummy);
        
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

        Presence                    = new()
        {
            CanBeSetAbsent          = true,
        };

        Corpse                      = new()
        {
            Name                    = nameof(DummyCorpse),
            FreshDuration           = 10,
            DecayDuration           = 10,
            ConsumeDuration         = 10,
            RemainsDuration         = 10,
        };

        Rendering                   = new()
        {
            DepthSortingTier        = SortTier.Ground,
        };
    }
}
