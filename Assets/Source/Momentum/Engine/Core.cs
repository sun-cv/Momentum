


namespace Game.Engine
{
    public class Core
    {
        private readonly Engine.Clock       clock;
        private readonly Engine.Tick        tick;
        private readonly Engine.Scheduler   scheduler;

        public Core()
        {
            clock       = new();
            tick        = new(clock);
            scheduler   = new(tick );
        }

        public void Tick()
        {
            tick.Execute();
        }        

        public void Late()
        {
            tick.Late();
        } 

        public void Shutdown()
        {
            scheduler.Dispose();
        }

        public Clock Clock => clock;
    }
}



