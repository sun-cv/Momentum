using UnityEngine;


namespace Momentum
{


    public interface IInputEvent : IEvent
    {
        public ButtonInput Button { get; }
    }

    public struct MousePosition         : IInputEvent { public readonly ButtonInput Button => ButtonInput.None; public Vector2 vector; public MousePosition(Vector2 value) => vector = value;   }
    public struct MovementFacing        : IInputEvent { public readonly ButtonInput Button => ButtonInput.None; public Principal facing; public MovementFacing(Principal value) => facing = value;   }
    public struct MovementVector        : IInputEvent { public readonly ButtonInput Button => ButtonInput.None; public Vector2 vector; public MovementVector(Vector2 value) => vector = value;  }

    public struct InteractPress         : IInputEvent { public readonly ButtonInput Button => ButtonInput.Dash;    }
    public struct InteractRelease       : IInputEvent { public readonly ButtonInput Button => ButtonInput.Dash;    }

    public struct AttackPress           : IInputEvent { public readonly ButtonInput Button => ButtonInput.Attack;  }
    public struct AttackRelease         : IInputEvent { public readonly ButtonInput Button => ButtonInput.Attack;  }

    public struct BlockPress            : IInputEvent { public readonly ButtonInput Button => ButtonInput.Block;   }
    public struct BlockRelease          : IInputEvent { public readonly ButtonInput Button => ButtonInput.Block;   }

    public struct DashPress             : IInputEvent { public readonly ButtonInput Button => ButtonInput.Dash;    }
    public struct DashRelease           : IInputEvent { public readonly ButtonInput Button => ButtonInput.Dash;    }

    public struct AimPress              : IInputEvent { public readonly ButtonInput Button => ButtonInput.Dash;    }
    public struct AimRelease            : IInputEvent { public readonly ButtonInput Button => ButtonInput.Dash;    }

}
