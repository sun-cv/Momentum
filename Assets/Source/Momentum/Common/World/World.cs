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
        Dodge,
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

    public interface IWorld {}

    public readonly struct Entity
    {
        public int Index        { get; init; }
        public int Generation   { get; init; } 

        public Entity(int index, int generation)
        {
            Index       = index;
            Generation  = generation;
        }
    }

    public readonly struct Blueprint
    {
        public Definition Definition    { get; init; }
        public GameObject Prefab        { get; init; }
    }

    public readonly struct ConstructionParameter
    {
        public Entity? Parent           { get; init; }
        public Vector3 Position         { get; init; }
        public Vector3 Rotation         { get; init; }
    }

}
