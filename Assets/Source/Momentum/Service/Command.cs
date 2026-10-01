using System.Collections.Generic;

using Game.Realm;
using Game.Common;
using Game.Diagnostic;



namespace Game.Service
{

    public class CommandSystem : RegisteredService, IWorld, IRealBase
    {

        private readonly World World; 

        public CommandSystem(World world)
        {
            World = world;
        }

        public void Tick()
        {
            ProcessCommands();
        }

        private void ProcessCommands()
        {
            foreach (var entity in World.Query(Mask<Components, Commands>.Key))
            {
                RemoveExpiredCommands(entity);
            }
        }

        private void RemoveExpiredCommands(Entity entity)
        {
            CleanActiveQueue(World.Entity.Command(entity).Active);
            CleanBufferQueue(World.Entity.Command(entity).Buffer);
        }

        private void CleanActiveQueue(Dictionary<Capability, Command>queue)
        {
            List<Capability> remove = new();

            foreach (var (capability, command) in queue)
            {
                if (command.Released)
                {
                    remove.Add(capability);
                }
            }
            
            foreach(var capability in remove)
            {
                queue.Remove(capability);
            }
        }

        private void CleanBufferQueue(Dictionary<Capability, Command>queue)
        {
            List<Capability> remove = new();

            foreach (var (capability, command) in queue)
            {
                if (command.Released && Watch.Tick.Real - command.TickReleased >= Config.Command.ReleaseWindow)
                {
                    remove.Add(capability);
                }
                
                if (!command.Released && Watch.Tick.Real - command.TickPressed >= Config.Command.PressWindow)
                {
                    remove.Add(capability);
                }
            }
            
            foreach(var capability in remove)
            {
                queue.Remove(capability);
            }
        }

        static CommandSystem() => Log<CommandSystem>.Level(Diagnostic.Log.Level.Debug);

    }
}
