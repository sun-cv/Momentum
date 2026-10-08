using System;
using System.Collections.Generic;
using UnityEngine;



namespace Game.Common
{
    public enum Capability
    {
        Use,
        Interact,
        Action,
        Attack,
        Primary,
        Secondary,
        Modifier,
        Dash,
        Parry,
        Move,
        Yield,
        Carry,
        Rotate,
        Equip,
        Cast,
        Rest,
        Heal,
        Repair,
        Charge,
        Regenerate,
        Recharge,
        Teleport,
    }

    public enum AbilityTag
    {
        Action,
        Toggle,
        Instant,
        Movement
    }


    public interface IWorld {}

    public readonly struct Entity : IEquatable<Entity>
    {
        public int Index                    { get; init; }
        public int Generation               { get; init; }

        public Entity(int index, int generation)
        {
            Index       = index;
            Generation  = generation;
        }

        public bool Equals(Entity other)                    => Index == other.Index && Generation == other.Generation;
        public override bool Equals(object instance)        => instance is Entity other && Equals(other);
        public override int GetHashCode()                   => HashCode.Combine(Index, Generation);

        public static bool operator ==(Entity a, Entity b)  =>  a.Equals(b);
        public static bool operator !=(Entity a, Entity b)  => !a.Equals(b);
    }

    public readonly struct Blueprint
    {
        public Definition Definition        { get; init; }
        public GameObject Prefab            { get; init; }
        public IReadOnlyList<Sheet> Sheets  { get; init; }
    }

    public readonly struct ConstructionParameter
    {
        public Entity? Parent               { get; init; }
        public Vector3 Position             { get; init; }
        public Vector3 Rotation             { get; init; }
    }

}
