using Game.Realm;
using Game.Common;
using Game.Content;
using Game.Diagnostic;
using Game.Common.Events;
using Game.Development;



namespace Game.Service
{

    public class Dev : RegisteredService, IWorld, IData, IAsset, IInitialize, IRealBase
    {

        private readonly Data Data;
        private readonly Assets Asset;
        private readonly World World;

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
    
        public void Tick() 
        {

        }

        private void Loggers()
        {
            Log.Actor.Movement  (World, hero);
            Log.Actor.Command   (World, hero);
            Log.Actor.Ability   (World, hero);
            Log.Actor.Effects   (World, hero);
            Log.Actor.Stats     (World, hero);
            Log.Actor.Resources (World, hero);
        }

        static Dev() => Log<Dev>.Level(Diagnostic.Log.Level.Debug);                
    }
}
