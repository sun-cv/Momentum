using System.Collections.Generic;
using UnityEngine;



namespace Game.Common
{
    public interface IComponent {}

    public struct Meta              : IComponent 
    {
        public int Created                              { get; set; }
    }

    public struct Ledger            : IComponent {}
    
    public struct Parent            : IComponent 
    {
        public Entity Entity                            { get; set; }
    }

    public struct Child             : IComponent
    {
        public List<Entity> Entities                    { get; set; }
    }

    public struct Source            : IComponent
    {
        public Entity Entity                            { get; set; }
    }

    public struct Prop              : IComponent {}
    public struct Actor             : IComponent {}
    public struct Corpse            : IComponent {}
    public struct Spawner           : IComponent {}
    public struct Projectile        : IComponent {}
    public struct Directive         : IComponent {}
    public struct Effect            : IComponent {}

    public struct Faction           : IComponent {}
    public struct Allegiance        : IComponent {}
    public struct Temperament       : IComponent {}

    public struct Item              : IComponent {}
    public struct Openable          : IComponent {}
    public struct Container         : IComponent {}
    public struct Interactable      : IComponent {}

    public struct Slowed            : IComponent {}

    public struct Stunned           : IComponent {}
    public struct Cold              : IComponent {}
    public struct Freezing          : IComponent {}
    public struct Hot               : IComponent {}
    public struct Burning           : IComponent {}
    public struct Charging          : IComponent {}
    public struct Electrified       : IComponent {}

    public struct Explosive         : IComponent {}
    public struct Destructible      : IComponent {}

    public struct AiController      : IComponent {}
    public struct PlayerController  : IComponent {}

    public struct Innate            : IComponent 
    {
        public List<Capability> Capabilities            { get; set; }
    }

    public struct Blocks            : IComponent 
    {
        public List<Capability> Capabilities            { get; set; }
    }

    public struct CommandQueue      : IComponent
    {
        public Dictionary<Capability, Command> Active   { get; set; }
        public Dictionary<Capability, Command> Buffer   { get; set; }
    }

    public struct Abilities         : IComponent
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

    public struct Physics           : IComponent 
    {
        public Vector2 Force                            { get; set; }
    }

    public struct Force             : IComponent {}
    public struct Contact           : IComponent {}
    public struct Collision         : IComponent {}
    public struct Velocity          : IComponent 
    {
        public Vector2 Value                            { get; set; }
    }
    public struct Mass              : IComponent 
    {
        public float Drag                               { get; set; }
        public float Weight                             { get; set; }
        public float Friction                           { get; set; }
        public float Momentum                           { get; set; }
    }

    public struct Movement          : IComponent
    {
        public float Speed                              { get; set; }
        public float Acceleration                       { get; set; }
    }

    public struct Intent            : IComponent
    {
        public Vector2 Direction                        { get; set; }
    }

    public struct Control           : IComponent
    {
        public Vector2 Velocity                         { get; set; }
        public float Modifier                           { get; set; }
    }

    public struct Impulse           : IComponent 
    {
        public Vector2 Velocity                         { get; set; }
    }

    public struct Kinematic         : IComponent 
    {
        public Vector2 Velocity                         { get; set; }
    }

    public struct Displacement      : IComponent 
    {
        public Vector2 Direction                        { get; set; }
        public float Distance                           { get; set; }
        public float Speed                              { get; set; }
        public float SteerRate                          { get; set; }
        public int Duration                             { get; set; }
        public int Progress                             { get; set; }
    }

    public struct SpeedModifier     : IComponent
    {
        public float Value                              { get; set; }
    }

    public struct AttackModifier    : IComponent
    {
        public float Value                              { get; set; }
    }

    public struct Aim               : IComponent
    {
        public Vector2 Direction                        { get; set; }
    }

    public struct Health            : IComponent
    {
        public int Current                              { get; set; }
        public int Maximum                              { get; set; }
    }

    public struct Energy            : IComponent
    {
        public int Current                              { get; set; }
        public int Maximum                              { get; set; }
    }

    public struct TimeScale         : IComponent 
    {
        public float Scale                              { get; set; } 
    }

    public struct TimeModifier      : IComponent 
    {
        public float Value                              { get; set; }     
    }

    public struct Instance          : IComponent 
    {
        public Transform Transform                      { get; set; }
    }

    public struct Body              : IComponent 
    {
        public Rigidbody2D Form                         { get; set; }
    }

    public struct Animation         : IComponent
    {
        public Animator Animator                        { get; set; }
    }

    public struct Rendering         : IComponent
    {
        public SpriteRenderer Renderer                  { get; set; }
    }

    public struct Sorting           : IComponent        
    {
        public Collider2D Layer                         { get; set; }
        public Collider2D Front                         { get; set; }
        public Collider2D Back                          { get; set; }
    }

    public struct HurtBox           : IComponent
    {
        public Collider2D Collider                      { get; set; }
    }
    

}
