


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
            DepthSortingTier        = SortTier.Prop,
        };
    }
}

[Definition]
public class DummyCorpseDefinition : ActorDefinition
{
    public DummyCorpseDefinition()
    {
        Name                        = nameof(DummyCorpse);
        
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
