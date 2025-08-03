

namespace Momentum
{

public interface ICameraBehavior
{
    void Initialize(CameraContext context);
    void Tick();
    void TickLate();
}

}