using System.Collections.Generic;
using Game.Common;
using Game.Diagnostic;
using Game.Realm;
using UnityEngine;
using Event = Game.Common.Event;


namespace Game.Service
{
   
    public class PlayerInputSystem : RegisteredService, IRealBase, IWorld
    {
        private readonly World World; 

        private Vector2 intent;
        private Vector2 aim;

        public PlayerInputSystem(World world)
        {
            World = world;
            Event.Register<PlayerInputSystem, IntentVector>();
        }

        public void Tick()
        {
            ReadIntent();
            UpdateIntent();
        }

        private void ReadIntent()
        {
            var messages = Event.Read<PlayerInputSystem, IntentVector>();

            foreach (var message in messages)
            {
                intent = message.Vector;
            }
        }

        private void UpdateIntent()
        {
            IReadOnlyCollection<Entity> ids = World.Query(Mask<PlayerController>.Key);

            foreach ( var entity in ids )
            {
                World.Entity.Component.Modify.Intent(entity).Direction = intent;
            }
        }

        static PlayerInputSystem() => Log<PlayerInputSystem>.Level(Diagnostic.Log.Level.Debug);
    }
}


