


namespace Game.Common
{
    public enum Capability
    {
        Interact,
        Action,
        Attack1,
        Attack2,
        Modifier,
        Movement,
        Dodge,
        Rotate,
        Use,
        Menu,
        Item,
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

}
