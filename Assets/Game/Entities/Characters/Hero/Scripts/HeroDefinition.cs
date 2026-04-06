



[Definition]
public class HeroDefinition : ActorDefinition
{
    public HeroDefinition()
    {
        Name                        = nameof(Hero);

        Stats                       = new()
        {
            Health                  = new()
            {
                AlertOnChange       = true,
            },
            MaxHealth               = 100,
            HealthRegen             = 5,

            Armor                   = new()
            {
                AlertOnChange       = true,
            },
            MaxArmor                = 0,

            Shield                  = new()
            {
                AlertOnChange       = true,
            },
            MaxShield               = 100,
            ShieldRegen             = 5,

            Energy                  = new()
            {
                AlertOnChange       = true,
            },
            MaxEnergy               = 100,
            EnergyRegen             = 5,

            Speed                   = 5,
            Strength                = 10,
            Impact                  = 10,
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
                Corpse              = ""
            },
            Respawn                 = new()
            {
                Enabled             = false
            },
        };

        Appearance                  = new()
        {

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
            },
            DepthSortingTier        = SortTier.Entity,
        };

    }
}
