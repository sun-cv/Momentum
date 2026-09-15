


using System.Collections.Generic;
using UnityEngine;

namespace Game.Common
{
    public interface IComponent {}

    public struct Meta              : IComponent 
    {
        public string Id            { get; set; }
        public int Created          { get; set; }
    }

    public struct Ledger            : IComponent {}
    
    public struct Prop              : IComponent {}
    public struct Actor             : IComponent {}
    public struct Corpse            : IComponent {}
    public struct Spawner           : IComponent {}
    public struct Projectile        : IComponent {}
    
    public struct Faction           : IComponent {}
    public struct Allegiance        : IComponent {}
    public struct Temperament       : IComponent {}

    public struct Physics           : IComponent 
    {
        public Vector2 Force        { get; set; }
    }

    public struct Force             : IComponent {}
    public struct Contact           : IComponent {}
    public struct Collision         : IComponent {}
    public struct Transform         : IComponent 
    {
        public Vector2 Position     { get; set; }
    }

    public struct Item              : IComponent {}
    public struct Openable          : IComponent {}
    public struct Container         : IComponent {}
    public struct Interactable      : IComponent {}

    public struct Explosive         : IComponent {}
    public struct Destructible      : IComponent {}

    public struct AiController      : IComponent {}
    public struct PlayerController  : IComponent {}

    public struct Intent            : IComponent
    {
        public Vector2 Direction    { get; set; }
    }

    public struct Movement          : IComponent
    {
        public Vector2 Directon     { get; set; }
    }

    public struct CommandQueue      : IComponent
    {
        public Dictionary<Capability, Command> Active { get; set; }
        public Dictionary<Capability, Command> Buffer { get; set; }
    }

    public struct Abilities     : IComponent
    {

    }

    public struct Loadout           : IComponent
    {

    }

    public struct Equipment         : IComponent
    {

    }

    public struct Inventory         : IComponent
    {

    }


    public struct Target            : IComponent
    {

    }

    public struct Aim               : IComponent
    {
        public Vector2 Direction    { get; set; }
    }

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

    public struct TimeScale         : IComponent 
    {
        public float Scale          { get; set; } 
        public float Duration       { get; set; } 
        public float BaseScale      { get; set; } 
    }



}
