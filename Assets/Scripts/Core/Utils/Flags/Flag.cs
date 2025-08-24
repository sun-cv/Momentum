

namespace Momentum
{

    public class Flag 
    {
        public bool Value { get; protected set; }
        public static implicit operator bool(Flag flag) => flag.Value;

    }

}