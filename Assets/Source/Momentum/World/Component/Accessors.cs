using Game.Common;



namespace Game.Realm
{

    public partial class Components
    {
        public sealed class Modifier
        {
            readonly Components Component;

            internal Modifier(Components components)
            {
                Component = components;
            }

            public ref Meta Meta(Entity entity)                             => ref Component.Reference<Meta>(entity);

            public ref Ledger Ledger(Entity entity)                         => ref Component.Reference<Ledger>(entity);

            public ref Parent Parent(Entity entity)                         => ref Component.Reference<Parent>(entity);
            public ref Child Child(Entity entity)                           => ref Component.Reference<Child>(entity);
            public ref Source Source(Entity entity)                         => ref Component.Reference<Source>(entity);

            public ref Prop Prop(Entity entity)                             => ref Component.Reference<Prop>(entity);
            public ref Actor Actor(Entity entity)                           => ref Component.Reference<Actor>(entity);
            public ref Corpse Corpse(Entity entity)                         => ref Component.Reference<Corpse>(entity);
            public ref Spawner Spawner(Entity entity)                       => ref Component.Reference<Spawner>(entity);
            public ref Projectile Projectile(Entity entity)                 => ref Component.Reference<Projectile>(entity);
            public ref Directive Directive(Entity entity)                   => ref Component.Reference<Directive>(entity);
            public ref Effect Effect(Entity entity)                         => ref Component.Reference<Effect>(entity);

            public ref Faction Faction(Entity entity)                       => ref Component.Reference<Faction>(entity);
            public ref Allegiance Allegiance(Entity entity)                 => ref Component.Reference<Allegiance>(entity);
            public ref Temperament Temperament(Entity entity)               => ref Component.Reference<Temperament>(entity);

            public ref Item Item(Entity entity)                             => ref Component.Reference<Item>(entity);
            public ref Openable Openable(Entity entity)                     => ref Component.Reference<Openable>(entity);
            public ref Container Container(Entity entity)                   => ref Component.Reference<Container>(entity);
            public ref Interactable Interactable(Entity entity)             => ref Component.Reference<Interactable>(entity);

            public ref Slowed Slowed(Entity entity)                         => ref Component.Reference<Slowed>(entity);

            public ref Stunned Stunned(Entity entity)                       => ref Component.Reference<Stunned>(entity);
            public ref Cold Cold(Entity entity)                             => ref Component.Reference<Cold>(entity);
            public ref Freezing Freezing(Entity entity)                     => ref Component.Reference<Freezing>(entity);
            public ref Hot Hot(Entity entity)                               => ref Component.Reference<Hot>(entity);
            public ref Burning Burning(Entity entity)                       => ref Component.Reference<Burning>(entity);
            public ref Charging Charging(Entity entity)                     => ref Component.Reference<Charging>(entity);
            public ref Electrified Electrified(Entity entity)               => ref Component.Reference<Electrified>(entity);

            public ref Explosive Explosive(Entity entity)                   => ref Component.Reference<Explosive>(entity);
            public ref Destructible Destructible(Entity entity)             => ref Component.Reference<Destructible>(entity);

            public ref AiController AiController(Entity entity)             => ref Component.Reference<AiController>(entity);
            public ref PlayerController PlayerController(Entity entity)     => ref Component.Reference<PlayerController>(entity);

            public ref Innate Innate(Entity entity)                         => ref Component.Reference<Innate>(entity);
            public ref Blocks Blocks(Entity entity)                         => ref Component.Reference<Blocks>(entity);

            public ref CommandQueue Command(Entity entity)                  => ref Component.Reference<CommandQueue>(entity);

            public ref Abilities Abilities(Entity entity)                   => ref Component.Reference<Abilities>(entity);
            public ref Loadout Loadout(Entity entity)                       => ref Component.Reference<Loadout>(entity);
            public ref Equipment Equipment(Entity entity)                   => ref Component.Reference<Equipment>(entity);
            public ref Inventory Inventory(Entity entity)                   => ref Component.Reference<Inventory>(entity);

            public ref Target Target(Entity entity)                         => ref Component.Reference<Target>(entity);

            public ref Physics Physics(Entity entity)                       => ref Component.Reference<Physics>(entity);
            public ref Force Force(Entity entity)                           => ref Component.Reference<Force>(entity);
            public ref Contact Contact(Entity entity)                       => ref Component.Reference<Contact>(entity);
            public ref Collision Collision(Entity entity)                   => ref Component.Reference<Collision>(entity);
            public ref Velocity Velocity(Entity entity)                     => ref Component.Reference<Velocity>(entity);
            public ref Mass Mass(Entity entity)                             => ref Component.Reference<Mass>(entity);

            public ref Movement Movement(Entity entity)                     => ref Component.Reference<Movement>(entity);
            public ref Intent Intent(Entity entity)                         => ref Component.Reference<Intent>(entity);
            public ref Control Control(Entity entity)                       => ref Component.Reference<Control>(entity);
            public ref Impulse Impulse(Entity entity)                       => ref Component.Reference<Impulse>(entity);
            public ref Kinematic Kinematic(Entity entity)                   => ref Component.Reference<Kinematic>(entity);
            public ref Displacement Displacement(Entity entity)             => ref Component.Reference<Displacement>(entity);

            public ref SpeedModifier SpeedModifier(Entity entity)           => ref Component.Reference<SpeedModifier>(entity);
            public ref AttackModifier AttackModifier(Entity entity)         => ref Component.Reference<AttackModifier>(entity);

            public ref Aim Aim(Entity entity)                               => ref Component.Reference<Aim>(entity);

            public ref Health Health(Entity entity)                         => ref Component.Reference<Health>(entity);
            public ref Energy Energy(Entity entity)                         => ref Component.Reference<Energy>(entity);

            public ref TimeScale TimeScale(Entity entity)                   => ref Component.Reference<TimeScale>(entity);

            public ref Instance Instance(Entity entity)                     => ref Component.Reference<Instance>(entity);
            public ref Body Body(Entity entity)                             => ref Component.Reference<Body>(entity);
            public ref Animation Animation(Entity entity)                   => ref Component.Reference<Animation>(entity);
            public ref Rendering Rendering(Entity entity)                   => ref Component.Reference<Rendering>(entity);
            public ref Sorting Sorting(Entity entity)                       => ref Component.Reference<Sorting>(entity);
            public ref HurtBox HurtBox(Entity entity)                       => ref Component.Reference<HurtBox>(entity);

            public ref CameraTarget CameraRigTarget(Entity entity)          => ref Component.Reference<CameraTarget>(entity);
        }
    }


    public partial class Entities
    {
        public Meta Meta(Entity entity)                                     => Component.View<Meta>(entity);

        public Ledger Ledger(Entity entity)                                 => Component.View<Ledger>(entity);

        public Parent Parent(Entity entity)                                 => Component.View<Parent>(entity);
        public Child Child(Entity entity)                                   => Component.View<Child>(entity);
        public Source Source(Entity entity)                                 => Component.View<Source>(entity);

        public Prop Prop(Entity entity)                                     => Component.View<Prop>(entity);
        public Actor Actor(Entity entity)                                   => Component.View<Actor>(entity);
        public Corpse Corpse(Entity entity)                                 => Component.View<Corpse>(entity);
        public Spawner Spawner(Entity entity)                               => Component.View<Spawner>(entity);
        public Projectile Projectile(Entity entity)                         => Component.View<Projectile>(entity);
        public Directive Directive(Entity entity)                           => Component.View<Directive>(entity);
        public Effect Effect(Entity entity)                                 => Component.View<Effect>(entity);

        public Faction Faction(Entity entity)                               => Component.View<Faction>(entity);
        public Allegiance Allegiance(Entity entity)                         => Component.View<Allegiance>(entity);
        public Temperament Temperament(Entity entity)                       => Component.View<Temperament>(entity);

        public Item Item(Entity entity)                                     => Component.View<Item>(entity);
        public Openable Openable(Entity entity)                             => Component.View<Openable>(entity);
        public Container Container(Entity entity)                           => Component.View<Container>(entity);
        public Interactable Interactable(Entity entity)                     => Component.View<Interactable>(entity);

        public Slowed Slowed(Entity entity)                                 => Component.View<Slowed>(entity);

        public Stunned Stunned(Entity entity)                               => Component.View<Stunned>(entity);
        public Cold Cold(Entity entity)                                     => Component.View<Cold>(entity);
        public Freezing Freezing(Entity entity)                             => Component.View<Freezing>(entity);
        public Hot Hot(Entity entity)                                       => Component.View<Hot>(entity);
        public Burning Burning(Entity entity)                               => Component.View<Burning>(entity);
        public Charging Charging(Entity entity)                             => Component.View<Charging>(entity);
        public Electrified Electrified(Entity entity)                       => Component.View<Electrified>(entity);

        public Explosive Explosive(Entity entity)                           => Component.View<Explosive>(entity);
        public Destructible Destructible(Entity entity)                     => Component.View<Destructible>(entity);

        public AiController AiController(Entity entity)                     => Component.View<AiController>(entity);
        public PlayerController PlayerController(Entity entity)             => Component.View<PlayerController>(entity);

        public Innate Innate(Entity entity)                                 => Component.View<Innate>(entity);
        public Blocks Blocks(Entity entity)                                 => Component.View<Blocks>(entity);

        public CommandQueue Command(Entity entity)                          => Component.View<CommandQueue>(entity);

        public Abilities Abilities(Entity entity)                           => Component.View<Abilities>(entity);
        public Loadout Loadout(Entity entity)                               => Component.View<Loadout>(entity);
        public Equipment Equipment(Entity entity)                           => Component.View<Equipment>(entity);
        public Inventory Inventory(Entity entity)                           => Component.View<Inventory>(entity);

        public Target Target(Entity entity)                                 => Component.View<Target>(entity);

        public Physics Physics(Entity entity)                               => Component.View<Physics>(entity);
        public Force Force(Entity entity)                                   => Component.View<Force>(entity);
        public Contact Contact(Entity entity)                               => Component.View<Contact>(entity);
        public Collision Collision(Entity entity)                           => Component.View<Collision>(entity);
        public Velocity Velocity(Entity entity)                             => Component.View<Velocity>(entity);
        public Mass Mass(Entity entity)                                     => Component.View<Mass>(entity);

        public Movement Movement(Entity entity)                             => Component.View<Movement>(entity);
        public Intent Intent(Entity entity)                                 => Component.View<Intent>(entity);
        public Control Control(Entity entity)                               => Component.View<Control>(entity);
        public Impulse Impulse(Entity entity)                               => Component.View<Impulse>(entity);
        public Kinematic Kinematic(Entity entity)                           => Component.View<Kinematic>(entity);
        public Displacement Displacement(Entity entity)                     => Component.View<Displacement>(entity);

        public SpeedModifier SpeedModifier(Entity entity)                   => Component.View<SpeedModifier>(entity);
        public AttackModifier AttackModifier(Entity entity)                 => Component.View<AttackModifier>(entity);

        public Aim Aim(Entity entity)                                       => Component.View<Aim>(entity);

        public Health Health(Entity entity)                                 => Component.View<Health>(entity);
        public Energy Energy(Entity entity)                                 => Component.View<Energy>(entity);

        public TimeScale TimeScale(Entity entity)                           => Component.View<TimeScale>(entity);

        public Instance Instance(Entity entity)                             => Component.View<Instance>(entity);
        public Body Body(Entity entity)                                     => Component.View<Body>(entity);
        public Animation Animation(Entity entity)                           => Component.View<Animation>(entity);
        public Rendering Rendering(Entity entity)                           => Component.View<Rendering>(entity);
        public Sorting Sorting(Entity entity)                               => Component.View<Sorting>(entity);
        public HurtBox HurtBox(Entity entity)                               => Component.View<HurtBox>(entity);

        public CameraTarget CameraRigTarget(Entity entity)                  => Component.View<CameraTarget>(entity);
    }

    public partial class Entities
    {
        public bool CanUse(Entity entity)                                   => capabilities.Can(entity, Common.Capability.Use);
        public bool CanInteract(Entity entity)                              => capabilities.Can(entity, Common.Capability.Interact);
        public bool CanAction(Entity entity)                                => capabilities.Can(entity, Common.Capability.Action);

        public bool CanAttack(Entity entity)                                => capabilities.Can(entity, Common.Capability.Attack);
        public bool CanPrimary(Entity entity)                               => capabilities.Can(entity, Common.Capability.Primary);
        public bool CanSecondary(Entity entity)                             => capabilities.Can(entity, Common.Capability.Secondary);
        public bool CanModifier(Entity entity)                              => capabilities.Can(entity, Common.Capability.Modifier);

        public bool CanDodge(Entity entity)                                 => capabilities.Can(entity, Common.Capability.Dodge);
        public bool CanMove(Entity entity)                                  => capabilities.Can(entity, Common.Capability.Move);
        public bool CanRotate(Entity entity)                                => capabilities.Can(entity, Common.Capability.Rotate);

        public bool CanEquip(Entity entity)                                 => capabilities.Can(entity, Common.Capability.Equip);
        public bool CanCast(Entity entity)                                  => capabilities.Can(entity, Common.Capability.Cast);
        public bool CanRest(Entity entity)                                  => capabilities.Can(entity, Common.Capability.Rest);

        public bool CanHeal(Entity entity)                                  => capabilities.Can(entity, Common.Capability.Heal);
        public bool CanRepair(Entity entity)                                => capabilities.Can(entity, Common.Capability.Repair);
        public bool CanCharge(Entity entity)                                => capabilities.Can(entity, Common.Capability.Charge);
        public bool CanRegenerate(Entity entity)                            => capabilities.Can(entity, Common.Capability.Regenerate);
        public bool CanRecharge(Entity entity)                              => capabilities.Can(entity, Common.Capability.Recharge);

        public bool CanTeleport(Entity entity)                              => capabilities.Can(entity, Common.Capability.Teleport);
    }
}
