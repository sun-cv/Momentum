



[Definition]
public class HeroDefinition : ActorDefinition
{
    public HeroDefinition()
    {
        Name                        = nameof(Hero);
        
        Stats                       = new()
        {
            MaxHealth               = 100,
            MaxArmor                = 0,
            MaxShield               = 100,
            MaxEnergy               = 100,
            ShieldRegen             = 5,
            EnergyRegen             = 5,
            HealthRegen             = 5,
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

        Lifecycle                   = new()
        {
            Spawn                   = new()
            {
                Corpse              = true
            },
            Respawn                 = new()
            {
                Enabled             = false
            },
        };

        Animations                  = new()
        {
            Spawn                   = new()
            {
                Enabled             = false,
                Default             = "Spawn"
            },

            Death                   = new()
            {
                Enabled             = false,
                Default             = "Death"
            },
        };

        Rendering                   = new()
        {
            DepthSortingTier        = SortTier.Entity,
        };
    }
}
