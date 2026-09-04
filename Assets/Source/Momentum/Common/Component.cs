


namespace Game.Common
{
    public interface IComponent {}

    public struct Health : IComponent
    {
        public int Current { get; set; }
        public int Maximum { get; set; }
    }
}
