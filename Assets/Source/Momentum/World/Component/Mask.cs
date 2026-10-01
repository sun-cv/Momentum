using System;
using System.Collections.Generic;
using Game.Common;


namespace Game.Realm
{

    internal static class MaskIndex
    {
        private static readonly Dictionary<Type, int> indexes = new();

        public static int Next<TDomain>()
        {
            if (!indexes.TryGetValue(typeof(TDomain), out var _))
            {
                indexes[typeof(TDomain)] = 0;
            }

            return indexes[typeof(TDomain)]++;
        }
    }

    internal static class MaskBit<TDomain, TValue>
    {
        public static readonly int Index = MaskIndex.Next<TDomain>();  
    } 

    public struct Mask<TDomain> : IEquatable<Mask<TDomain>>
    {
        public ulong Bits0, Bits1, Bits2, Bits3;

        public readonly Mask<TDomain> With<TValue>()
        {
            int index   = MaskBit<TDomain, TValue>.Index;
            ulong flag  = 1UL << (index % 64);
            var mask    = this;

            switch (index / 64)
            {
                case 0: mask.Bits0 |= flag; break;
                case 1: mask.Bits1 |= flag; break;
                case 2: mask.Bits2 |= flag; break;
                case 3: mask.Bits3 |= flag; break;
            }
            return mask;
        }

        public readonly Mask<TDomain> With(int index)
        {
            ulong flag  = 1UL << (index % 64);
            var mask    = this;

            switch (index / 64)
            {
                case 0: mask.Bits0 |= flag; break;
                case 1: mask.Bits1 |= flag; break;
                case 2: mask.Bits2 |= flag; break;
                case 3: mask.Bits3 |= flag; break;
            }
            return mask;
        }


        public readonly Mask<TDomain> Without<TValue>()
        {
            int index   = MaskBit<TDomain, TValue>.Index;
            ulong flag  = 1UL << (index % 64);
            var mask    = this;

            switch (index / 64)
            {
                case 0: mask.Bits0 &= ~flag; break;
                case 1: mask.Bits1 &= ~flag; break;
                case 2: mask.Bits2 &= ~flag; break;
                case 3: mask.Bits3 &= ~flag; break;
            }
            return mask;
        }

        public readonly bool Contains(Mask<TDomain> required)
        {
            return  (Bits0 & required.Bits0) == required.Bits0 &&
                    (Bits1 & required.Bits1) == required.Bits1 &&
                    (Bits2 & required.Bits2) == required.Bits2 &&
                    (Bits3 & required.Bits3) == required.Bits3;
        }

        public readonly Mask<TOther> To<TOther>()
        {
            return new Mask<TOther> { Bits0 = Bits0, Bits1 = Bits1, Bits2 = Bits2, Bits3 = Bits3 };
        }

        public readonly Mask<TDomain> Without(int index)
        {
            ulong flag  = 1UL << (index % 64);
            var mask    = this;

            switch (index / 64)
            {
                case 0: mask.Bits0 &= ~flag; break;
                case 1: mask.Bits1 &= ~flag; break;
                case 2: mask.Bits2 &= ~flag; break;
                case 3: mask.Bits3 &= ~flag; break;
            }
            return mask;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Bits0, Bits1, Bits2, Bits3);
        }

        public readonly bool Equals(Mask<TDomain> other)
        {
            return Bits0 == other.Bits0 && Bits1 == other.Bits1 && Bits2 == other.Bits2 && Bits3 == other.Bits3;
        }

        public readonly override bool Equals(object obj)
        {
            return obj is Mask<TDomain> m && Equals(m);
        }
    }

    public static class Mask<TDomain, T1> where T1 : IComponent
    {
        public static readonly Mask<TDomain> Key = new Mask<TDomain>().With<T1>();
    }

    public static class Mask<TDomain, T1, T2>
        where T1 : IComponent where T2 : IComponent
        {
            public static readonly Mask<TDomain> Key = new Mask<TDomain>().With<T1>().With<T2>();
        }

    public static class Mask<TDomain, T1, T2, T3>
        where T1 : IComponent where T2 : IComponent where T3 : IComponent
        {
            public static readonly Mask<TDomain> Key = new Mask<TDomain>().With<T1>().With<T2>().With<T3>();
        }

    public static class Mask<TDomain, T1, T2, T3, T4>
        where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent
        {
            public static readonly Mask<TDomain> Key = new Mask<TDomain>().With<T1>().With<T2>().With<T3>().With<T4>();
        }

    public static class Mask<TDomain, T1, T2, T3, T4, T5>
        where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent
        {
            public static readonly Mask<TDomain> Key = new Mask<TDomain>().With<T1>().With<T2>().With<T3>().With<T4>().With<T5>();
        }

    public static class Mask<TDomain, T1, T2, T3, T4, T5, T6>
        where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent
        {
            public static readonly Mask<TDomain> Key = new Mask<TDomain>().With<T1>().With<T2>().With<T3>().With<T4>().With<T5>().With<T6>();
        }
}
