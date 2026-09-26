using System.Collections.Generic;
using UnityEngine;



namespace Game.Common
{
    public interface IComponent {}

    public struct Meta              : IComponent 
    {
        public int Created                                  { get; set; }
    }

    public struct Ledger            : IComponent {}
    
    public struct Parent            : IComponent 
    {
        public Entity Entity                                { get; set; }
    }

    public struct Child             : IComponent
    {
        public List<Entity> Entities                        { get; set; }
    }

    public struct Anchor            : IComponent
    {
        public Vector2 Offset                               { get; set; }
    }

    public struct Source            : IComponent
    {
        public Entity Entity                                { get; set; }
    }

    public struct Duration          : IComponent
    {
        public int Length                                   { get; set; }
        public int Elapsed                                  { get; set; }
    }

    public struct Prop              : IComponent {}
    public struct Actor             : IComponent {}
    public struct Corpse            : IComponent {}
    public struct Spawner           : IComponent {}
    public struct Projectile        : IComponent {}
    public struct Directive         : IComponent {}
    public struct Effect            : IComponent {}
    public struct Hitbox            : IComponent {}
    public struct Ability           : IComponent {}

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
    public struct Shocked           : IComponent {}
    public struct Electrified       : IComponent {}

    public struct Explosive         : IComponent {}
    public struct Destructible      : IComponent {}
    
    public struct Unstoppable       : IComponent {}
    public struct Anchored          : IComponent {}

    public struct AiController      : IComponent {}
    public struct PlayerController  : IComponent {}

    public struct Innate            : IComponent 
    {
        public IReadOnlyList<Capability> Capabilities       { get; set; }
    }

    public struct Blocks            : IComponent 
    {
        public IReadOnlyList<Capability> Capabilities       { get; set; }
    }

    public struct CommandQueue      : IComponent
    {
        public Dictionary<Capability, Command> Active       { get; set; }
        public Dictionary<Capability, Command> Buffer       { get; set; }
    }

    public struct Abilities         : IComponent
    {

    }

    public struct Hitboxes          : IComponent
    {
        public List<HitboxEntry> Entries                    { get; set; }
    }
    
    public struct Struck            : IComponent
    {
        public List<Entity> Entities                        { get ; set; }
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
        public Entity Entity                                { get; set; }
    }

    public struct Physics           : IComponent 
    {
        public Vector2 Force                                { get; set; }
    }

    public struct Force             : IComponent {}
    public struct Contact           : IComponent {}
    public struct Collision         : IComponent {}

    public struct Velocity          : IComponent 
    {
        public Vector2 Value                                { get; set; }
    }

    public struct Mass              : IComponent 
    {
        public float Weight                                 { get; set; }
        public float Friction                               { get; set; }
    }

    public struct Movement          : IComponent
    {
        public float Speed                                  { get; set; }
        public float Acceleration                           { get; set; }
    }

    public struct Intent            : IComponent
    {
        public Vector2 Direction                            { get; set; }
    }

    public struct Control           : IComponent
    {
        public Vector2 Velocity                             { get; set; }
        public float Modifier                               { get; set; }
    }

    public struct Impulse           : IComponent 
    {
        public Vector2 Velocity                             { get; set; }
    }

    public struct Kinematic         : IComponent 
    {
        public Vector2 Velocity                             { get; set; }
    }

    public struct Displacement      : IComponent 
    {
        public Vector2 Direction                            { get; set; }
        public float Distance                               { get; set; }
        public float Speed                                  { get; set; }
        public float SteerRate                              { get; set; }
        public int Duration                                 { get; set; }
        public int Progress                                 { get; set; }
    }

    public struct Aim               : IComponent
    {
        public Vector2 Direction                            { get; set; }
        public Vector2 World                                { get; set; }
    }

    public struct Track             : IComponent {}


    public struct Health            : IComponent
    {
        public int Current                                  { get; set; }
        public int Maximum                                  { get; set; }
    }

    public struct Armor             : IComponent
    {
        public int Current                                  { get; set; }
        public int Maximum                                  { get; set; }
    }

    public struct Energy            : IComponent
    {
        public int Current                                  { get; set; }
        public int Maximum                                  { get; set; }
    }

    public struct Parry             : IComponent
    {
        public int Start                                    { get; set; }
    }

    public struct Damage            : IComponent
    {
        public int Amount                                   { get; set; }
    }
    
    public struct TimeScale         : IComponent 
    {
        public float Scale                                  { get; set; } 
    }

    public struct Instance          : IComponent 
    {
        public Transform Transform                          { get; set; }
    }

    public struct Form              : IComponent 
    {
        public Rigidbody2D Body                             { get; set; }
    }

    public struct Visual            : IComponent
    {
        public Transform Transform                          { get; set; }
        public Vector2 Current                              { get; set; }
        public Vector2 Previous                             { get; set; }
    }

    public struct Facing            : IComponent
    {

    }

    public struct Animation         : IComponent
    {
        public Animator Animator                            { get; set; }
    }

    public struct Rendering         : IComponent
    {
        public SpriteRenderer Renderer                      { get; set; }
    }

    public struct HurtBox           : IComponent
    {
        public Collider2D Collider                          { get; set; }
    }
    
    public struct CameraTarget      : IComponent {}

    public struct SpeedModifier     : IComponent
    {
        public float Value                                  { get; set; }
    }

    public struct AttackModifier    : IComponent
    {
        public float Value                                  { get; set; }
    }

    public struct TimeModifier      : IComponent 
    {
        public float Scale                                  { get; set; }     
    }

}
