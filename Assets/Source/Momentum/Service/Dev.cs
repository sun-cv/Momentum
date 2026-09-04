using Game.Realm;
using Game.Common;
using Game.Diagnostic;
using System.Collections.Generic;
using System.Linq;



namespace Game.Service
{

    public class Dev : RegisteredService, IRateBase, IRateHalf, IRateStep
    {

        private World World;

        private List<Entity> entities = new();

        public Dev()
        {
            World = new();
        }
    
        void IRateBase.Tick() 
        {
        }

        void IRateHalf.Tick() 
        {
            if (entities.Count() == 10)
            {
                entities.ForEach(id => World.Entity.Release(id));
                entities.Clear();
            }
        }

        void IRateStep.Tick() 
        {
            var id = World.Entity.Create();
            entities.Add(id);
            Log<Dev>.Debug($"Entity | Index: {id.Index} Generation: {id.Generation} ");
        }

        static Dev() => Log<Dev>.Level(Diagnostic.Log.Level.Debug);                
    }
}
