

namespace Momentum
{

    public interface ITickAll : ITick, ITickFixed, ITickLate {}

    public interface ITick      { public void Tick();       }
    public interface ITickFixed { public void TickFixed();  }
    public interface ITickLate  { public void TickLate();   }


}