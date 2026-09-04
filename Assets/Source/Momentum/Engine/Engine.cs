


namespace Game.Core
{
    public class Engine
    {
        private readonly Clock      clock;
        private readonly Execute    execute;
        private readonly Scheduler  scheduler;
        private readonly Scanner    scanner;

        public Engine()
        {
            clock       = new();
            execute     = new(clock);
            scheduler   = new(execute);
            scanner     = new();
        }

        public void Tick()
        {
            clock   .Tick();
            execute .Tick();
        }        

        public void Late()
        {
            clock   .Late();
            execute .Late();
        } 

        public void Shutdown()
        {
            scheduler.Dispose();
        }

        public Clock Clock => clock;
    }
}



