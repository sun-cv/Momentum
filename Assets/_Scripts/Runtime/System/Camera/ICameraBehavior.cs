using Momentum.Cameras;


namespace Momentum.Interface
{

public interface ICameraBehavior
{
    void Initialize(CameraContext context);
    void Tick();
    void TickLate();
}

}