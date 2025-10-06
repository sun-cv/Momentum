

namespace Momentum
{
    public interface ITrait {}

    public interface ICancellable   : ITrait { }
    public interface IInterruptible : ITrait { }
    public interface IBufferable    : ITrait { }

    public interface IDamageable    : ITrait
    {
        // public void TakeDamage(DamageInstance instance);
    }


}
