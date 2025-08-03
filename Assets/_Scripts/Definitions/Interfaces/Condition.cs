

namespace Momentum
{

    public interface ISlow      {};
    public interface IStun      {};
    public interface IKnockback {};
    public interface IStagger   {};
    public interface IBreak     {};
    public interface IShock     {};

    public enum Condition
    {
        Slowed,
        Stunned,
        Knockedback,
        Staggered,
        Broken,
        Shocked,
    }

}