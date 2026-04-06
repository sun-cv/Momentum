


[Definition]
public class DummyDefinition : ActorDefinition
{
    public DummyDefinition()
    {
        Name                        = nameof(Dummy);
        
        Stats                       = new()
        {

            Health                  = new()
            {
                AlertOnChange       = true,
            },
            MaxHealth               = 100,
            MaxEnergy               = 100,
            Speed                   = 0,
        };

        Physics                     = new()
        {
            Mass                    = 1000,

        };

        Lifecycle                   = new()
        {   
            Spawn                   = new()
            {
                Corpse              = nameof(DummyCorpse),
            },
            Respawn                 = new()
            {
                Enabled             = false,
            },
        };

        Appearance                  = new()
        {
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
            },
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
            },
        };

        Appearance                  = new()
        {
            DepthSortingTier        = SortTier.Ground,
        };
    }
}
