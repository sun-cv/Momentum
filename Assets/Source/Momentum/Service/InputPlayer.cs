using System.Collections.Generic;
using System.Linq;
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

        public PlayerInputSystem(World world)
        {
            World = world;
            Event.Register<PlayerInputSystem, IntentVector>();
        }

        public void Tick()
        {
            UpdatePlayerInput();
        }


        private void UpdatePlayerInput()
        {
            var intent      = Event.Read<PlayerInputSystem, IntentVector>();
            var capability  = Event.Read<PlayerInputSystem, InputEvent>();

            ProcessPlayerIntent(intent);      
            ProcessPlayerCapability(capability); 
        }


        private void ProcessPlayerIntent(List<IntentVector> messages)
        {
            foreach (var message in messages)
            {
                UpdateIntent(message.Vector);
            }
        }

        private void UpdateIntent(Vector2 vector)
        {
            var players = World.Query(Mask<PlayerController>.Key);

            foreach ( var player in players )
            {
                World.Entity.Component.Modify.Intent(player).Direction = vector;
            }
        }

        private void ProcessPlayerCapability(List<InputEvent> messages)
        {
             
            foreach (var message in messages)
            {
                UpdateCapability(message.Capability, message.Pressed, message.Released);
            }
        }

        private void UpdateCapability(Capability capability, bool pressed, bool released)
        {
            var players = World.Query(Mask<PlayerController>.Key);

            foreach (var player in players)
            {
                if (pressed)
                {
                    World.Entity.Modify.Command(player).Buffer[capability] = new Command(){ Capability = capability, TickPressed = Watch.Tick.Real };
                }

                if (released)
                {
                    if (World.Entity.Modify.Command(player).Active.TryGetValue(capability, out var active))
                    {
                        active.Released     = true;
                        active.TickReleased = Watch.Tick.Real;
                    }

                    if (World.Entity.Modify.Command(player).Buffer.TryGetValue(capability, out var buffer))
                    {
                        buffer.Released     = true;
                        buffer.TickReleased = Watch.Tick.Real;
                    }
                }
            }
        }

        static PlayerInputSystem() => Log<PlayerInputSystem>.Level(Diagnostic.Log.Level.Debug);
    }
}


