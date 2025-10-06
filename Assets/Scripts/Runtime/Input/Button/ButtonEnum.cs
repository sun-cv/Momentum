

namespace Momentum
{

    public enum ButtonInput
    {
        None,
        Interact,
        Attack,
        Block,
        Dash,
        Aim,
    }

    public enum ButtonCondition
    {
        None,
        PressedThisFrame,
        Pressed,
        Held,
        ReleasedThisFrame,
        ReleasedRecently,
    }

}