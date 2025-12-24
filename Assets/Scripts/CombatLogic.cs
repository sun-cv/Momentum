



using UnityEngine;

public class Combat
{

    [Function]
    public static void CalculateDamage(Entity victim, Entity attacker, object args)
    {

    }   

    [Function]
    public static void ApplyDamage(Entity victim, int damage)
    {
        if (victim is IDamageable damageable)
            if (damageable.Invulnerable)
                return;

        if (victim is IHasHealth applicable)
            applicable.Health -= damage;
    }




}





