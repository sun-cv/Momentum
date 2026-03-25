


public static partial class Rules
{
    public static class Damage
    {
        public static readonly ShieldDamageRules    Shield  = new();
        public static readonly ArmorDamageRules     Armor   = new();
        public static readonly HealthDamageRules    Health  = new();
    }

    public static class Animator
    {
        public static readonly AnimatorParameter Parameter  = new();
    }
}

