using System.Collections.Generic;
using System.Linq;



namespace Momentum
{

    public enum EntityPriority
    {
        Hero = 0,
        Enemy = 10,
        NPC = 20,
    }

    public class EntitySystem : IEntitySystem
    {

        IHero                                   hero;
        SortedDictionary<int, List<Entity>>     entities = new();


        public void Initialize()
        {
            AccessRegistry();

            hero.Initialize();
        }

        public void AccessRegistry()
        {
            hero = Registry.Get<IHero>();
        }

        public void Tick()
        {

            hero.Tick();

            foreach (var (priority, list) in entities)
            {
                foreach (var entity in list)
                {
                    entity.Tick();
                }
            }
        }

        public void TickLate()
        {
            hero.TickLate();
        }

        public void TickFixed()
        {
            hero.TickFixed();
        }
    


        public void Register(EntityPriority priority, Entity entity)
        {
            if (!entities.TryGetValue((int)priority, out var list))
            {
                list = new();
                entities.Add((int)priority, list);
            }

            list.Add(entity);
        }

        public void Deregister(Entity entity)
        {
            foreach (var key in entities.Keys.ToList())
            {
                var list = entities[key];
                list.Remove(entity);
                if (list.Count == 0)
                {
                    entities.Remove(key);
                }
            }
        }
    }
}