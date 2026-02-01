

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
        };
    }
}