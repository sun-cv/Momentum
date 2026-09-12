


namespace Game.Common
{

    public interface IRate              { }
    public interface IRealBase : IRate  { public void Tick(); };
    public interface IRealHalf : IRate  { public void Tick(); };
    public interface IRealStep : IRate  { public void Tick(); };
    public interface IRealUtil : IRate  { public void Tick(); };

    public interface IRealLate : IRate  { public void Tick(); };

    public interface IGameBase : IRate  { public void Tick(); };
    public interface IGameHalf : IRate  { public void Tick(); };
    public interface IGameStep : IRate  { public void Tick(); };
    public interface IGameUtil : IRate  { public void Tick(); };
    
    public enum TickRate 
    { 
        Base,
        Half,
        Step,
        Util,
        Late,
    }

    public enum TickMode
    {
        Real,
        Game,
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
