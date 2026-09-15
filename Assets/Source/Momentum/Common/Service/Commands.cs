


namespace Game.Common
{
    public class Command
    {
        public Capability Capability    { get; set; }

        public int TickPressed          { get; set; }
        public int TickReleased         { get; set; }

        public bool Released            { get; set; }
    }
}
