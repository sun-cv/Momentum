

public class HeroDefinition : EntityDefinition
{
    public HeroDefinition()
    {
        Name                            = "Hero";

        Health                          = 0;
        MaxHealth                       = 100;

        Mana                            = 100;
        MaxMana                         = 0;

        Speed                           = 5;

        Attack                          = 15;
    }
}