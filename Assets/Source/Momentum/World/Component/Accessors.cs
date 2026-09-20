using Game.Common;



namespace Game.Realm
{

    public partial class Components
    {
        public sealed class Modifier
        {
            readonly Components components;

            internal Modifier(Components components)
            {
                this.components = components;
            }

            public ref Meta Meta(Entity entity)                             => ref components.Reference<Meta>(entity);

            public ref Ledger Ledger(Entity entity)                         => ref components.Reference<Ledger>(entity);

            public ref Parent Parent(Entity entity)                         => ref components.Reference<Parent>(entity);
            public ref Child Child(Entity entity)                           => ref components.Reference<Child>(entity);
            public ref Source Source(Entity entity)                         => ref components.Reference<Source>(entity);

            public ref Prop Prop(Entity entity)                             => ref components.Reference<Prop>(entity);
            public ref Actor Actor(Entity entity)                           => ref components.Reference<Actor>(entity);
            public ref Spawner Spawner(Entity entity)                       => ref components.Reference<Spawner>(entity);
            public ref Projectile Projectile(Entity entity)                 => ref components.Reference<Projectile>(entity);
            public ref Corpse Corpse(Entity entity)                         => ref components.Reference<Corpse>(entity);

            public ref Innate Innate(Entity entity)                         => ref components.Reference<Innate>(entity);
            public ref Blocks Blocks(Entity entity)                         => ref components.Reference<Blocks>(entity);

            public ref Faction Faction(Entity entity)                       => ref components.Reference<Faction>(entity);
            public ref Allegiance Allegiance(Entity entity)                 => ref components.Reference<Allegiance>(entity);
            public ref Temperament Temperament(Entity entity)               => ref components.Reference<Temperament>(entity);

            public ref Physics Physics(Entity entity)                       => ref components.Reference<Physics>(entity);
            public ref Force Force(Entity entity)                           => ref components.Reference<Force>(entity);
            public ref Contact Contact(Entity entity)                       => ref components.Reference<Contact>(entity);
            public ref Collision Collision(Entity entity)                   => ref components.Reference<Collision>(entity);

            public ref Item Item(Entity entity)                             => ref components.Reference<Item>(entity);
            public ref Openable Openable(Entity entity)                     => ref components.Reference<Openable>(entity);
            public ref Container Container(Entity entity)                   => ref components.Reference<Container>(entity);
            public ref Interactable Interactable(Entity entity)             => ref components.Reference<Interactable>(entity);

            public ref Explosive Explosive(Entity entity)                   => ref components.Reference<Explosive>(entity);
            public ref Destructible Destructible(Entity entity)             => ref components.Reference<Destructible>(entity);

            public ref AiController AiController(Entity entity)             => ref components.Reference<AiController>(entity);
            public ref PlayerController PlayerController(Entity entity)     => ref components.Reference<PlayerController>(entity);

            public ref Intent Intent(Entity entity)                         => ref components.Reference<Intent>(entity);
            public ref Movement Movement(Entity entity)                     => ref components.Reference<Movement>(entity);
            public ref CommandQueue Command(Entity entity)                  => ref components.Reference<CommandQueue>(entity);

            public ref Abilities Abilities(Entity entity)                   => ref components.Reference<Abilities>(entity);
            public ref Loadout Loadout(Entity entity)                       => ref components.Reference<Loadout>(entity);
            public ref Equipment Equipment(Entity entity)                   => ref components.Reference<Equipment>(entity);
            public ref Inventory Inventory(Entity entity)                   => ref components.Reference<Inventory>(entity);

            public ref Target Target(Entity entity)                         => ref components.Reference<Target>(entity);
            public ref Aim Aim(Entity entity)                               => ref components.Reference<Aim>(entity);

            public ref Health Health(Entity entity)                         => ref components.Reference<Health>(entity);
            public ref Energy Energy(Entity entity)                         => ref components.Reference<Energy>(entity);

            public ref TimeScale TimeScale(Entity entity)                   => ref components.Reference<TimeScale>(entity);

            public ref Instance Instance(Entity entity)                     => ref components.Reference<Instance>(entity);
            public ref Body Body(Entity entity)                             => ref components.Reference<Body>(entity);
            public ref Animation Animation(Entity entity)                   => ref components.Reference<Animation>(entity);
            public ref Rendering Rendering(Entity entity)                   => ref components.Reference<Rendering>(entity);
            public ref Sorting Sorting(Entity entity)                       => ref components.Reference<Sorting>(entity);
            public ref HurtBox HurtBox(Entity entity)                       => ref components.Reference<HurtBox>(entity);
        }
    }


    public partial class Entities
    {
        public Meta Meta(Entity entity)                                     => component.View<Meta>(entity);

        public Ledger Ledger(Entity entity)                                 => component.View<Ledger>(entity);

        public Parent Parent(Entity entity)                                 => component.View<Parent>(entity);
        public Source Source(Entity entity)                                 => component.View<Source>(entity);
        public Child Child(Entity entity)                                   => component.View<Child>(entity);

        public Prop Prop(Entity entity)                                     => component.View<Prop>(entity);
        public Actor Actor(Entity entity)                                   => component.View<Actor>(entity);
        public Spawner Spawner(Entity entity)                               => component.View<Spawner>(entity);
        public Projectile Projectile(Entity entity)                         => component.View<Projectile>(entity);
        public Corpse Corpse(Entity entity)                                 => component.View<Corpse>(entity);

        public Innate Innate(Entity entity)                                 => component.View<Innate>(entity);
        public Blocks Blocks(Entity entity)                                 => component.View<Blocks>(entity);

        public Faction Faction(Entity entity)                               => component.View<Faction>(entity);
        public Allegiance Allegiance(Entity entity)                         => component.View<Allegiance>(entity);
        public Temperament Temperament(Entity entity)                       => component.View<Temperament>(entity);

        public Physics Physics(Entity entity)                               => component.View<Physics>(entity);
        public Force Force(Entity entity)                                   => component.View<Force>(entity);
        public Contact Contact(Entity entity)                               => component.View<Contact>(entity);
        public Collision Collision(Entity entity)                           => component.View<Collision>(entity);

        public Item Item(Entity entity)                                     => component.View<Item>(entity);
        public Openable Openable(Entity entity)                             => component.View<Openable>(entity);
        public Container Container(Entity entity)                           => component.View<Container>(entity);
        public Interactable Interactable(Entity entity)                     => component.View<Interactable>(entity);

        public Explosive Explosive(Entity entity)                           => component.View<Explosive>(entity);
        public Destructible Destructible(Entity entity)                     => component.View<Destructible>(entity);

        public AiController AiController(Entity entity)                     => component.View<AiController>(entity);
        public PlayerController PlayerController(Entity entity)             => component.View<PlayerController>(entity);
        
        public Intent Intent(Entity entity)                                 => component.View<Intent>(entity);
        public Movement Movement(Entity entity)                             => component.View<Movement>(entity);
        public CommandQueue Command(Entity entity)                          => component.View<CommandQueue>(entity);

        public Abilities Abilities(Entity entity)                           => component.View<Abilities>(entity);
        public Loadout Loadout(Entity entity)                               => component.View<Loadout>(entity);
        public Equipment Equipment(Entity entity)                           => component.View<Equipment>(entity);
        public Inventory Inventory(Entity entity)                           => component.View<Inventory>(entity);

        public Target Target(Entity entity)                                 => component.View<Target>(entity);
        public Aim Aim(Entity entity)                                       => component.View<Aim>(entity);

        public Health Health(Entity entity)                                 => component.View<Health>(entity);
        public Energy Energy(Entity entity)                                 => component.View<Energy>(entity);

        public TimeScale TimeScale(Entity entity)                           => component.View<TimeScale>(entity);

        public Instance Instance(Entity entity)                             => component.View<Instance>(entity);
        public Body Body(Entity entity)                                     => component.View<Body>(entity);
        public Animation Animation(Entity entity)                           => component.View<Animation>(entity);
        public Rendering Rendering(Entity entity)                           => component.View<Rendering>(entity);
        public Sorting Sorting(Entity entity)                               => component.View<Sorting>(entity);
        public HurtBox HurtBox(Entity entity)                               => component.View<HurtBox>(entity);
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
