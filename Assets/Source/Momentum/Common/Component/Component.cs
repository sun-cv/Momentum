


using UnityEngine;

namespace Game.Common
{
    public interface IComponent {}

    public struct Health            : IComponent
    {
        public int Current          { get; set; }
        public int Maximum          { get; set; }
    }

    public struct Energy            : IComponent
    {
        public int Current          { get; set; }
        public int Maximum          { get; set; }
    }

    public struct Intent            : IComponent
    {
        public Vector2 Direction    { get; set; }
    }

    public struct Aim               : IComponent
    {
        public Vector2 Directon     { get; set; }
    }

    public struct Command           : IComponent
    {
        
    }

    public struct Meta              : IComponent {}
    public struct Ledger            : IComponent {}
    
    public struct Spawner           : IComponent {}
    public struct Prop              : IComponent {}
    public struct Projectile        : IComponent {}
    public struct Actor             : IComponent {}
    public struct Corpse            : IComponent {}
    public struct Allegiance        : IComponent {}
    public struct Faction           : IComponent {}
    public struct Temperament       : IComponent {}

    public struct Interactable      : IComponent {}
    public struct Pushable          : IComponent {}
    public struct Openable          : IComponent {}

    public struct Item              : IComponent {}
    public struct Container         : IComponent {}

    public struct PlayerController  : IComponent {}

    public struct AiController      : IComponent {}

}
