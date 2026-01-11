using UnityEngine;




public class Combat
{

    [Function]
    public static void CalculateDamage(Actor victim, Actor attacker, object args)
    {

    }   

    [Function]
    public static void ApplyDamage(Actor victim, int damage)
    {
        if (victim is IDamageable damageable)
        {
            if (damageable.Invulnerable)
                return;

            damageable.Health -= damage;
        }
    }


}





