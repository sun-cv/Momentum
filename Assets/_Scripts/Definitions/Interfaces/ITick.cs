



namespace Momentum
{
    public interface ITick
    {
        void Tick();
    }

    public interface ITickLate
    {
        void TickLate();
    }

    public interface ITickFixed
    {
        void TickFixed();
    }

    public interface ITickAll : ITick, ITickFixed, ITickLate {}
}