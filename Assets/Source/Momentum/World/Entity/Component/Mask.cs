using System;
using Game.Common;



namespace Game.Realm
{

    public static class ComponentRegistry
    {
        private static int index;

        public static int Next()
        {
            return index++;
        }
    }

    public static class ComponentId<TComponent> where TComponent : IComponent
    {
        public static readonly int Index = ComponentRegistry.Next();  
    } 

    public struct Mask : IEquatable<Mask>
    {
        public ulong Bits0, Bits1, Bits2, Bits3;

        public readonly Mask With<TComponent>() where TComponent : IComponent
        {
            int index   = ComponentId<TComponent>.Index;
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

        public readonly Mask Without<TComponent>() where TComponent : IComponent
        {
            int index   = ComponentId<TComponent>.Index;
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

        public readonly bool Contains(Mask required)
        {
            return  (Bits0 & required.Bits0) == required.Bits0 &&
                    (Bits1 & required.Bits1) == required.Bits1 &&
                    (Bits2 & required.Bits2) == required.Bits2 &&
                    (Bits3 & required.Bits3) == required.Bits3;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Bits0, Bits1, Bits2, Bits3);
        }

        public readonly bool Equals(Mask other)
        {
            return Bits0 == other.Bits0 && Bits1 == other.Bits1 && Bits2 == other.Bits2 && Bits3 == other.Bits3;
        }

        public readonly override bool Equals(object obj)
        {
            return obj is Mask m && Equals(m);
        }
    }

    public static class Mask<T1> where T1 : IComponent
    {
        public static readonly Mask Key = new Mask().With<T1>();
    }

    public static class Mask<T1, T2> where T1 : IComponent where T2 : IComponent
    {
        public static readonly Mask Key = new Mask().With<T1>().With<T2>();
    }

    public static class Mask<T1, T2, T3> where T1 : IComponent where T2 : IComponent where T3 : IComponent
    {
        public static readonly Mask Key = new Mask().With<T1>().With<T2>().With<T3>();
    }

    public static class Mask<T1, T2, T3, T4> where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent
    {
        public static readonly Mask Key = new Mask().With<T1>().With<T2>().With<T3>().With<T4>();
    }

    public static class Mask<T1, T2, T3, T4, T5> where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent
    {
        public static readonly Mask Key = new Mask().With<T1>().With<T2>().With<T3>().With<T4>().With<T5>();
    }
}
