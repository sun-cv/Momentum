using Game.Realm;
using Game.Common;
using Game.Content;
using Game.Diagnostic;
using System.Collections.Generic;
using System.Linq;



namespace Game.Service
{

    public class Dev : RegisteredService, IRealBase, IRealHalf, IRealStep, IGameBase, IInitialize
    {

        private readonly World World;
        private readonly Data Data;
        private readonly List<Entity> entities = new();

        private readonly Entity id;
        private readonly Actor actor; 

        private int realCount;
        private int gameCount;

        private TickCounter tick;
        private TimeCounter time;

        public Dev()
        {
            World   = new();
            Data    = new(); 

            Log<Dev>.Debug(World.Entity.Health(id).Current);
        }

        public void Initialize()
        {

            tick = new(TickMode.Game);
            time = new(TickMode.Real);

            time.CountDown(20);

            tick.Start();
            time.Start();
        }
    
        void IRealBase.Tick() 
        {
            Log<Dev>.Debug( "RealBase",     () => $"{realCount++}");
            Log<Dev>.Debug( "Timer.Time",   () => $"{time.Current}");
            Log<Dev>.Debug( "Timer.Tick",   () => $"{tick.Current}");
        }

        void IGameBase.Tick() 
        {
            Log<Dev>.Debug( "GameBase", () => $"{gameCount++}");
        }
        void IRealHalf.Tick() 
        {
            if (entities.Count() == 10)
            {
                entities.ForEach(id => World.Entity.Release(id));
                entities.Clear();
            }
        }

        void IRealStep.Tick() 
        {

            World.Entity.Modify.Health(id).Current += 1;

            var test = World.Query(Mask<Health, Energy>.Key);

            foreach (var ent in test)
            {
                Log<Dev>.Debug($"Health current: {World.Entity.Health(ent).Current}");
                Log<Dev>.Debug($"Energy current: {World.Entity.Energy(ent).Current}");
            }

            var ids = World.Entity.Create.Actor(Data.Lookup.Actor("Hero"));

            entities.Add(ids);
            Log<Dev>.Debug( "Entity", () => $"Entity | Index: {ids.Index} Generation: {ids.Generation} ");
        }

        static Dev() => Log<Dev>.Level(Diagnostic.Log.Level.Debug);                
    }
}
