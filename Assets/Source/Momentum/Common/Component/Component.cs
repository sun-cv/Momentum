


namespace Game.Common
{
    public interface IComponent {}

    public struct Health : IComponent
    {
        public int Current { get; set; }
        public int Maximum { get; set; }
    }

    public struct Energy : IComponent
    {
        public int Current { get; set; }
        public int Maximum { get; set; }
    }


    public struct Spawner       : IComponent {}
    public struct Prop          : IComponent {}
    public struct Projectile    : IComponent {}
    public struct Actor         : IComponent {}
    public struct Corpse        : IComponent {}
    public struct Allegiance    : IComponent {}
    public struct Faction       : IComponent {}
    public struct Temperament   : IComponent {}

    public struct Interactable  : IComponent {}
    public struct Pushable      : IComponent {}
    public struct Openable      : IComponent {}
}
