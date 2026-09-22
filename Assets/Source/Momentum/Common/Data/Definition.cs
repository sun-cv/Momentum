using UnityEngine;



namespace Game.Common
{

    public interface IData {}
    public interface IRecord {}

    public class Definition : IRecord
    {
        public string Id                                        { get; init; }
        public string Prefab                                    { get; init; }

        public Meta Meta                                        { get; init; }
        public Ledger? Ledger                                   { get; init; }

        public Prop? Prop                                       { get; init; }
        public Actor? Actor                                     { get; init; }
        public Corpse? Corpse                                   { get; init; }
        public Spawner? Spawner                                 { get; init; }
        public Projectile? Projectile                           { get; init; }
        public Directive? Directive                             { get; init; }
        public Effect? Effect                                   { get; init; }

        public Faction? Faction                                 { get; init; }
        public Allegiance? Allegiance                           { get; init; }
        public Temperament? Temperament                         { get; init; }

        public Item? Item                                       { get; init; }
        public Openable? Openable                               { get; init; }
        public Container? Container                             { get; init; }
        public Interactable? Interactable                       { get; init; }

        public Slowed? Slowed                                   { get; init; }

        public Stunned? Stunned                                 { get; init; }
        public Cold? Cold                                       { get; init; }
        public Freezing? Freezing                               { get; init; }
        public Hot? Hot                                         { get; init; }
        public Burning? Burning                                 { get; init; }
        public Charging? Charging                               { get; init; }
        public Electrified? Electrified                         { get; init; }

        public Explosive? Explosive                             { get; init; }
        public Destructible? Destructible                       { get; init; }

        public AiController? AiController                       { get; init; }
        public PlayerController? PlayerController               { get; init; }

        public Innate? Innate                                   { get; init; }
        public Blocks? Blocks                                   { get; init; }

        public CommandQueue? CommandQueue                       { get; init; }

        public Abilities? Abilities                             { get; init; }
        public Loadout? Loadout                                 { get; init; }
        public Equipment? Equipment                             { get; init; }
        public Inventory? Inventory                             { get; init; }

        public Target? Target                                   { get; init; }

        public Physics? Physics                                 { get; init; }
        public Force? Force                                     { get; init; }
        public Contact? Contact                                 { get; init; }
        public Collision? Collision                             { get; init; }
        public Velocity? Velocity                               { get; init; }
        public Mass? Mass                                       { get; init; }

        public Movement? Movement                               { get; init; }
        public Intent? Intent                                   { get; init; }
        public Control? Control                                 { get; init; }
        public Impulse? Impulse                                 { get; init; }
        public Kinematic? Kinematic                             { get; init; }
        public Displacement? Displacement                       { get; init; }



        
        public SpeedModifier? SpeedModifier                     { get; init; }
        public AttackModifier? AttackModifier                   { get; init; }

        public Aim? Aim                                         { get; init; }

        public Health? Health                                   { get; init; }
        public Energy? Energy                                   { get; init; }

        public TimeScale? TimeScale                             { get; init; }

        public Instance? Instance                               { get; init; }
        public Body? Body                                       { get; init; }
        public Animation? Animation                             { get; init; }
        public Rendering? Rendering                             { get; init; }
        public Sorting? Sorting                                 { get; init; }
        public HurtBox? HurtBox                                 { get; init; }
    }
}
