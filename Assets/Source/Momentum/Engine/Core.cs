


namespace Game.Engine
{
    public class Core
    {
        private readonly Engine.Clock       clock;
        private readonly Engine.Execute     execute;
        private readonly Engine.Scheduler   scheduler;
        private readonly Engine.Scanner     scanner;

        public Core()
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



