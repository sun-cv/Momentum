using Game.Realm;
using Game.Common;
using Game.Content;
using Game.Diagnostic;
using System.Linq;
using Game.Common.Events;



namespace Game.Service
{

    public class EffectSystem
    {
        static EffectSystem() => Log<EffectSystem>.Level(Diagnostic.Log.Level.Debug);
    }
    
    public class Dev : RegisteredService, IWorld, IData, IAsset, IInitialize, IRealBase, IRealHalf, IRealStep, IGameBase
    {

        private readonly Data Data;
        private readonly Assets Asset;
        private readonly World World;

        private readonly EffectSystem effect = new();

        private Entity hero;
        private Entity dummy;


        public Dev(World world, Data data, Assets asset)
        {
            Data    = data;
            Asset   = asset;
            World   = world;
        }

        public void Initialize()
        {
            hero    = World.Entity.Create(Asset.Get("Hero"), new());
            dummy   = World.Entity.Create(Asset.Get("Dummy"), new() { Position = new() { x = 4, y = 4, z = 0 }});
            Event.Register<Dev, EnergyChanged>();
        }
    
        void IRealBase.Tick() 
        {
            var tick = Watch.Tick.Game % 60;

            if (tick == 15)
            {
                Event.Send<Expend>(new() { Entity = hero, Amount = 20});
            }

            if (tick == 30)
            {
                Event.Send<Recharge>(new() { Entity = hero, Amount = 10});
            }

            Event.Read<Dev, EnergyChanged>().ForEach(message => Diagnostic.Log<Dev>.Debug(message.After));
        }

        void IGameBase.Tick() 
        {
            Log();
        }

        void IRealHalf.Tick() 
        {

        }

        void IRealStep.Tick() 
        {

        }


        private void Log()
        {
            Debug.Movement  (World, hero);
            Debug.Command   (World, hero);
            Debug.Ability   (World, hero);
            Debug.Effects   (World, hero);
            Debug.Stats     (World, hero);
            Debug.Resources (World, hero);
        }

        static Dev() => Log<Dev>.Level(Diagnostic.Log.Level.Debug);                
    }
}
