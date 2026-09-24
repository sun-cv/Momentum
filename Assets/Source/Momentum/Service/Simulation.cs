using UnityEngine;

using Game.Realm;
using Game.Common;



namespace Game.Service
{
    public class SimulationSystem : RegisteredService, IWorld, IGameBase
    {
        private readonly World World;

        public SimulationSystem(World world)
        {
            World = world;

            Physics2D.simulationMode = SimulationMode2D.Script;
        }

        public void Tick()
        {
            Record();

            Physics2D.Simulate(Watch.Tick.Delta);

            Capture();
        }

        private void Record()
        {
            foreach (var entity in World.Query(Mask<Components, Visual, Form>.Key))
            {
                ref var visual  = ref World.Entity.Modify.Visual(entity);
                visual.Previous = visual.Current;
            }
        }

        private void Capture()
        {
            foreach (var entity in World.Query(Mask<Components, Visual, Form>.Key))
            {
                World.Entity.Modify.Visual(entity).Current = World.Entity.Form(entity).Body.position;
            }
        }
    }
}

