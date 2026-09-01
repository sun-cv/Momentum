


namespace Game.Common
{

    public interface IRate              { }
    public interface IRateBase : IRate  { public void Tick(); };
    public interface IRateHalf : IRate  { public void Tick(); };
    public interface IRateStep : IRate  { public void Tick(); };
    public interface IRateUtil : IRate  { public void Tick(); };
    public interface IRateLate : IRate  { public void Tick(); };


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
