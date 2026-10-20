using System.Collections.Generic;
using UnityEngine;



namespace Game.Common
{
    public interface IComponent {}


    //
    //  Core
    //

    /// <summary>Tick the Entity was created</summary>
    public struct Meta              : IComponent 
    {
        public int Created                                  { get; set; }
    }

    /// <summary>Definition id; names and matches, never reads back</summary>
    public struct Identity          : IComponent
    {
        public string Id                                    { get; set; }
    }

    /// <summary>Parent Entity; Exclusive</summary>
    public struct Parent            : IComponent
    {
        public Entity Entity                                { get; set; }
    }

    /// <summary>Child Entities; Inclusive</summary>
    public struct Child             : IComponent
    {
        public List<Entity> Entities                        { get; set; }
    }

    /// <summary>Binds Entity lifecycle to any other Entity</summary>
    public struct Bound             : IComponent
    {
        public Entity Entity                                { get; set; }
    }

    /// <summary>Defines source Entity in gameplay</summary>
    public struct Source            : IComponent
    {
        public Entity Entity                                { get; set; }
    }

    /// <summary>Fixed lifetime in ticks; released at end</summary>
    public struct Duration          : IComponent
    {
        public int Length                                   { get; set; }
        public int Elapsed                                  { get; set; }
        public int Remaining                                { get; set; }
    }

    /// <summary>Repeating tick counter; acts when Elapsed wraps</summary>
    public struct Interval          : IComponent
    {
        public int Length                                   { get; set; }
        public int Elapsed                                  { get; set; }
    }

    public struct Ledger            : IComponent {}


    //
    //  Archetype
    //

    /// <summary>Static world object</summary>
    public struct Prop              : IComponent {}
    /// <summary>Controlled, living Entity</summary>
    public struct Actor             : IComponent {}
    /// <summary>Entity is a corpse; not one that leaves one</summary>
    public struct Corpse            : IComponent {}
    /// <summary>Creates other Entities</summary>
    public struct Spawner           : IComponent {}
    /// <summary>Free-moving Entity with Source, no Parent</summary>
    public struct Projectile        : IComponent {}
    /// <summary>Deals hits to what it overlaps</summary>
    public struct Hitbox            : IComponent {}
    /// <summary>Lasting change on its Parent</summary>
    public struct Effect            : IComponent {}
    /// <summary>Authored motion on its Parent</summary>
    public struct Directive         : IComponent {}
    /// <summary>Running ability on its Parent</summary>
    public struct Ability           : IComponent {}
    /// <summary>Ability on cooldown for its Parent while alive</summary>
    public struct Cooldown          : IComponent {}
    /// <summary>Camera focus on its Parent</summary>
    public struct Focus             : IComponent {} 
    /// <summary>Authored replacing entity to maintain window</summary>
    public struct Window            : IComponent {} 

    //
    //  Control
    //

    /// <summary>Commands written by AI</summary>
    public struct AiController      : IComponent {}
    /// <summary>Commands written by player input</summary>
    public struct PlayerController  : IComponent {}

    /// <summary>Defines core capability set available to Entity</summary>
    public struct Innate            : IComponent
    {
        public IReadOnlyList<Capability> Capabilities       { get; set; }
    }

    /// <summary>Disables listed capabilities; Listed on root or as effect</summary> 
    public struct Blocks            : IComponent 
    {
        public IReadOnlyList<Capability> Capabilities       { get; set; }
    }

    /// <summary>Pressed capabilities; Buffer waits, Active is held</summary>
    public struct Commands          : IComponent
    {
        public Dictionary<Capability, Command> Active       { get; set; }
        public Dictionary<Capability, Command> Buffer       { get; set; }
    }


    //
    //  Ability
    //

    /// <summary>Root ability per capability</summary>
    public struct Loadout           : IComponent
    {
        public Dictionary<Capability, string> Abilities     { get; set; }
    }

    /// <summary>Which press starts the ability, and its kind</summary>
    public struct Activation        : IComponent
    {
        public enum Trigger { Buffer, Active }

        public Trigger From                                 { get; set; }
        public AbilityTag Kind                              { get; set; }    
        public Capability Capability                        { get; set; }
    }

    /// <summary>Authored phase sequence; State doubles as animation</summary>
    public struct Phases            : IComponent
    {
        public List<PhaseEntry> Entry                       { get; set; }
    }

    public struct PhaseEntry
    {
        public string State                                 { get; set; }
        public int Duration                                 { get; set; }
        public IReadOnlyList<Capability> UntilRelease       { get; set; }
    }

    /// <summary>Current phase and ticks spent in it</summary>
    public struct Phase             : IComponent
    {
        public int Index                                    { get; set; }
        public int Elapsed                                  { get; set; }
    }

    /// <summary>What may start while this ability runs</summary>
    public struct Controls          : IComponent
    {
        public enum Result { Deny, Cancel, Coexist }

        public IReadOnlyList<ControlEntry> Entry            { get; set; }
    }

    public struct ControlEntry
    {
        public int After;
        public string State;
        public AbilityTag Kind;
        public Controls.Result Result;
    }

    /// <summary>Capabilities that must stay held until a phase</summary>
    public struct Sustain           : IComponent
    {
        public IReadOnlyList<SustainEntry> Entry            { get; set; }
    }

    public struct SustainEntry
    {
        public Capability Capability                        { get; set; }
        public string Until                                 { get; set; }
    }

    /// <summary>Abilities this one leads into; Window holds them open after</summary>
    public struct Chains            : IComponent
    {
        public int Window                                   { get; set; }
        public Dictionary<Capability, string> Abilities     { get; set; }
    }

    /// <summary>Copies live Aim from Parent until a phase</summary>
    public struct Track             : IComponent            
    {
        public string Until                                 { get; set; }
    }

    /// <summary>Cooldowns started at a phase</summary>
    public struct Cooldowns         : IComponent 
    {
        public IReadOnlyList<CooldownEntry> Entry           { get; set; }
    }

    public struct CooldownEntry
    {
        public int Tick;
        public int Duration;
        public string State;
        public string Ability;
    }

    /// <summary>Ability id this cooldown holds</summary>
    public struct CooldownTarget    : IComponent 
    {
        public string Ability                               { get; set; }
    }

    /// <summary>Named hit fragments for hitbox entries</summary>
    public struct Payloads          : IComponent
    {
        public Dictionary<string, Definition> Entry         { get; set; }
    }


    //
    //  Ability Timeline
    //

    /// <summary>Hitboxes spawned at phase and tick</summary>
    public struct Hitboxes          : IComponent
    {
        public List<HitboxEntry> Entry                      { get; set; }
    }
    
    public struct HitboxEntry
    {
        public int Tick;
        public int Duration;
        public string State;
        public string Definition;
        public List<string> Payloads;
    }

    /// <summary>Effects created on Root at phase and tick</summary>
    public struct Effects           : IComponent 
    {
        public List<EffectEntry> Entry                      { get; set; }
    }

    public struct EffectEntry
    {
        public int Tick;
        public string State;
        public string Until;
        public List<Definition> Applies;
    }

    /// <summary>Directives created at phase and tick</summary>
    public struct Directives        : IComponent
    {
        public List<DirectiveEntry> Entry                   { get; set; }
    }

    public struct DirectiveEntry
    {
        public int Tick;
        public string State;
        public List<Definition> Applies;
    }

    /// <summary>Camera cues sent at phase and tick</summary>
    public struct CameraCues        : IComponent
    {
        public List<CameraCueEntry> Entry                   { get; set; }
    }

    public struct CameraCueEntry
    {
        public int Tick;
        public string State;
        public string Until;
        public Cue Cue;
    }

    //
    //  Hit
    //

    /// <summary>Entities already hit; built fresh per Entity</summary>
    public struct Struck            : IComponent
    {
        public List<Entity> Entities                        { get; set; }
    }

    /// <summary>Damage dealt once on hit</summary>
    public struct Damage            : IComponent
    {
        public int Amount                                   { get; set; }
    }

    /// <summary>Effects created on the target on hit</summary>
    public struct Applies           : IComponent 
    {
        public List<Definition> Entry                       { get; set; }
    }


    //
    //  Effect
    //

    /// <summary>Effect kind; control</summary>
    public struct CC                : IComponent {}
    /// <summary>Effect kind; damage over time</summary>
    public struct DOT               : IComponent {}
    /// <summary>Effect kind; stat increase</summary>
    public struct Buff              : IComponent {}
    /// <summary>Effect kind; stat decrease</summary>
    public struct Debuff            : IComponent {}
    /// <summary>Effect kind; stat decrease</summary>
    public struct StatusEffect      : IComponent {}


    /// <summary>Effect that stuns its Parent</summary>
    public struct Stun              : IComponent {}
    /// <summary>Entity is stunned</summary>
    public struct Stunned           : IComponent {}
    /// <summary>Effect that slows its Parent</summary>
    public struct Slow              : IComponent {}
    /// <summary>Entity is slowed</summary>
    public struct Slowed            : IComponent {}
    /// <summary>Parry window on its Parent</summary>
    public struct Parry             : IComponent {}

    public struct Knockback         : IComponent {}
    public struct Unstoppable       : IComponent {}
    public struct Anchored          : IComponent {}


    /// <summary>Signed speed percent folded into Parent</summary>
    public struct SpeedModifier     : IComponent
    {
        public float Value                                  { get; set; }
    }

    /// <summary>Signed time percent folded into Parent</summary>
    public struct TimeModifier      : IComponent 
    {
        public float Scale                                  { get; set; }     
    }


    //
    //  Element
    //

    /// <summary>Frost element an effect carries</summary>
    public struct Frost             : IComponent {}
    /// <summary>Entity is freezing</summary>
    public struct Freezing          : IComponent {}

    /// <summary>Burn element an effect carries</summary>
    public struct Burn              : IComponent {}
    /// <summary>Entity is burning</summary>
    public struct Burning           : IComponent {}

    /// <summary>Shock element an effect carries</summary>
    public struct Shock             : IComponent {}
    /// <summary>Entity is shocked</summary>
    public struct Shocked           : IComponent {}


    //
    //  Resource
    //

    public struct Stat
    {
        public float Base;
        public float Final;
    }

    public struct Stats             : IComponent
    {
        public Stat Health;
        public Stat Armor;
        public Stat Energy;
        public Stat Damage;
        public Stat CritChance;
        public Stat CritDamage;
        public Stat Speed;
    }

    public struct Modifier
    {
        public float Flat       { get; set; }
        public float Positive   { get; set; }
        public float Negative   { get; set; }
    }

    public struct Modifiers         : IComponent
    {
        public Modifier Health;
        public Modifier Armor;
        public Modifier Energy;
        public Modifier Damage;
        public Modifier CritChance;
        public Modifier CritDamage;
        public Modifier Speed;
    }

    /// <summary>Health pool; zero is death</summary>
    public struct Health            : IComponent
    {
        public int Current                                  { get; set; }
        public int Pending                                  { get; set; }
    }

    /// <summary>Armor pool in front of Health</summary>
    public struct Armor             : IComponent
    {
        public int Current                                  { get; set; }
        public int Pending                                  { get; set; }
    }

    /// <summary>Spent on skills and chaining</summary>
    public struct Energy            : IComponent
    {
        public int Current                                  { get; set; }
        public int Pending                                  { get; set; }
    }

    public struct Speed             : IComponent
    {
        public int Current                                  { get; set; }
    }

    public struct Acceleration      : IComponent
    {
        public int Current                                  { get; set; }
    }

    //
    //  Movement
    //

    /// <summary>Weight and friction for physical response</summary>
    public struct Mass              : IComponent 
    {
        public float Weight                                 { get; set; }
        public float Friction                               { get; set; }
    }

    /// <summary>Desired move direction</summary>
    public struct Intent            : IComponent
    {
        public Vector2 Direction                            { get; set; }
    }

    /// <summary>Controller-driven velocity after modifiers</summary>
    public struct Control           : IComponent
    {
        public Vector2 Velocity                             { get; set; }
    }

    /// <summary>External push, decays by Mass</summary>
    public struct Impulse           : IComponent 
    {
        public Vector2 Velocity                             { get; set; }
    }

    public struct Kinematic         : IComponent 
    {
        public Vector2 Velocity                             { get; set; }
    }

    /// <summary>Summed velocity for the tick</summary>
    public struct Velocity          : IComponent 
    {
        public Vector2 Value                                { get; set; }
    }

    /// <summary>Eased move over a set distance and ticks</summary>
    public struct Displacement      : IComponent 
    {
        public Vector2 Direction                            { get; set; }
        public float Ease                                   { get; set; }
        public float Distance                               { get; set; }
        public int Duration                                 { get; set; }
        public int Progress                                 { get; set; }
    }

    /// <summary>Steers toward Intent or Aim</summary>
    public struct Heading           : IComponent
    {
        public enum Source { Intent, Aim }

        public Source Toward                                { get; set; }
        public float SteerRate                              { get; set; }
    }

    /// <summary>Anchors Entity position to Root() Entity</summary>
    public struct Anchor            : IComponent
    {
        public Vector2 Offset                               { get; set; }
    }

    /// <summary>Entity time rate; 0 is stasis</summary>
    public struct TimeScale         : IComponent 
    {
        public float Scale                                  { get; set; } 
    }

    public struct Physics           : IComponent 
    {
        public Vector2 Force                                { get; set; }
    }

    public struct Force             : IComponent {}
    public struct Contact           : IComponent {}
    public struct Collision         : IComponent {}


    //
    //  Orientation
    //

    /// <summary>Aim direction and world point</summary>
    public struct Aim               : IComponent
    {
        public Vector2 Direction                            { get; set; }
        public Vector2 World                                { get; set; }
    }

    /// <summary>Targeted Entity</summary>
    public struct Target            : IComponent
    {
        public Entity Entity                                { get; set; }
    }

    /// <summary>Cardinal facing with turn delay</summary>
    public struct Facing            : IComponent
    {
        public Vector2 Direction                            { get; set; }
        public bool Horizontal                              { get; set; }
        public int Ticks                                    { get; set; }
    }


    //
    //  Unity
    //

    /// <summary>Unity Transform</summary>
    public struct Instance          : IComponent 
    {
        public Transform Transform                          { get; set; }
    }

    /// <summary>Unity Rigidbody2D</summary>
    public struct Form              : IComponent 
    {
        public Rigidbody2D Body                             { get; set; }
    }

    /// <summary>Unity Collider that receives hits</summary>
    public struct Hurtbox           : IComponent
    {
        public Collider2D Collider                          { get; set; }
    }
    
    /// <summary>Unity Collider for movement</summary>
    public struct Bodybox           : IComponent
    {
        public Collider2D Collider                          { get; set; }
    }


    //
    //  Presentation
    //

    /// <summary>Interpolated render Transform</summary>
    public struct Visual            : IComponent
    {
        public Transform Transform                          { get; set; }
        public Vector2 Current                              { get; set; }
        public Vector2 Previous                             { get; set; }
    }

    /// <summary>Sheet and state this Entity wants drawn</summary>
    public struct Pose              : IComponent
    {
        public string Sheet                                 { get; set; }
        public string State                                 { get; set; }
    }

    /// <summary>Resolved sheet, state and frame on the drawer</summary>
    public struct Animation         : IComponent
    {
        public Entity Source                                { get; set; }
        public string Sheet                                 { get; set; }
        public string State                                 { get; set; }
        public int Elapsed                                  { get; set; }
        public Vector2 Shown                                { get; set; }
    }

    /// <summary>SpriteRenderer and sorting layer</summary>
    public struct Rendering         : IComponent
    {
        public SpriteRenderer Renderer                      { get; set; }
        public string Layer                                 { get; set; }
    }

    /// <summary>Camera follows this Entity</summary>
    public struct CameraTarget      : IComponent {}

    /// <summary>Entity a Focus highlights; added after Create</summary>
    public struct CameraFocus       : IComponent
    {
        public Entity Entity                                { get; set; }
    }

    /// <summary>Camera direction toward this Entity, or toward its CameraFocus</summary>
    public struct Highlight         : IComponent
    {
        public float Takeover                               { get; set; }
        public float Center                                 { get; set; }
        public Vector2 Offset                               { get; set; }
        public float Range                                  { get; set; }
        public int Stickiness                               { get; set; }
        public int In                                       { get; set; }
        public int Out                                      { get; set; }
        public float Zoom                                   { get; set; }
        public float Limit                                  { get; set; }
    }

    public readonly struct Cue
    {
        public string Id                                    { get; init; }
        public string From                                  { get; init; }
        public int In                                       { get; init; }
        public int Hold                                     { get; init; }
        public int Out                                      { get; init; }
        public bool Sustained                               { get; init; }
        public float Radius                                 { get; init; }
        public Shake Shake                                  { get; init; }
        public float Zoom                                   { get; init; }
    }

    public readonly struct Shake
    {
        public float Amplitude                              { get; init; }
        public float Frequency                              { get; init; }
    }

    //
    //  Interaction
    //

    public struct Faction           : IComponent {}
    public struct Allegiance        : IComponent {}
    public struct Temperament       : IComponent {}

    public struct Item              : IComponent {}
    public struct Openable          : IComponent {}
    public struct Container         : IComponent {}
    public struct Interactable      : IComponent {}
    public struct Explosive         : IComponent {}
    public struct Destructible      : IComponent {}


    //
    //  Progression
    //

    public struct Price             : IComponent {}

    public struct Talent            : IComponent {}

    public struct Equipped          : IComponent {}

    public struct Equipment         : IComponent {}

    public struct Inventory         : IComponent {}
}

