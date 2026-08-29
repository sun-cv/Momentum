


namespace Game.Engine
{
    public class Core
    {
        private Engine.Clock        clock;
        private Engine.Scheduler    scheduler;

        public void Initialize()
        {
            clock       = new();
            scheduler   = new();

            scheduler.Initialize(clock);
        }

        public void Tick()
        {
            clock.Tick();
        }        

        public void Late()
        {
            clock.Late();
        } 

        public void Shutdown()
        {
            clock       .Dispose();
            scheduler   .Dispose();
        }
    }
}



