


using System;
using System.Text;
using UnityEngine;

namespace Game.Common
{
    public delegate void StatStep(string name, ref Stat stat);
    public delegate void StatPair(string name, ref Stat target, in Stat source);

    public struct Stat
    {
        public float Base;
        public float Value;
        public float Flat;
        public float Positive;
        public float Negative;
    }

    public static class StatResolver
    {
        public static void Each(ref Stats stats, StatStep step)
        {
            step(nameof(Stats.Health), ref stats.Health);
            step(nameof(Stats.Armor), ref stats.Armor);
            step(nameof(Stats.Attack), ref stats.Attack);
            step(nameof(Stats.CritChance), ref stats.CritChance);
            step(nameof(Stats.CritDamage), ref stats.CritDamage);
        }

        public static void Each(ref Stats target, in Stats source, StatPair step)
        {
            step(nameof(Stats.Health), ref target.Health, in source.Health);
            step(nameof(Stats.Armor), ref target.Armor, in source.Armor);
            step(nameof(Stats.Attack), ref target.Attack, in source.Attack);
            step(nameof(Stats.CritChance), ref target.CritChance, in source.CritChance);
            step(nameof(Stats.CritDamage), ref target.CritDamage, in source.CritDamage);
        }

        public static void Default(string name, ref Stat stat)
        {
            stat.Value = stat.Base;
        }

        public static void Reset(string name, ref Stat stat)
        {
            stat.Flat = 0f;
            stat.Positive = 0f;
            stat.Negative = 0f;
        }

        public static void Apply(string name, ref Stat stat)
        {
            stat.Value = (stat.Base + stat.Flat) * (1f + stat.Negative) * (1f + stat.Positive);
        }

        public static void Combine(string name, ref Stat target, in Stat source)
        {
            target.Flat += source.Flat;
            target.Positive = Math.Max(target.Positive, source.Positive);
            target.Negative = Math.Min(target.Negative, source.Negative);
        }

        public static string Describe(in Stat stat)
        {
            return $"{stat.Value}";
        }
    }
}
