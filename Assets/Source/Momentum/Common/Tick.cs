


namespace Game.Common
{

    public interface IRate              { }
    internal interface IRateBase : IRate  { internal void Tick(); };
    internal interface IRateHalf : IRate  { internal void Tick(); };
    interan interface IRateStep : IRate  { internal void Tick(); };
    public interface IRateUtil : IRate  { internal void Tick(); };
    public interface IRateLate : IRate  { internal void Tick(); };


    public enum TickRate 
    { 
        Base,
        Half,
        Step,
        Util,
        Late,
    }

    public enum TickPhase
    {
        System,
        Input,
        Logic,
        Physics,
        Resolve,
        Render
    }
}
