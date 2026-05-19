using System.Collections.Generic;



public static partial class IntentMap
{
    public static readonly Dictionary<Capability, Trigger> Triggers = new()
    {
        { Capability.None,     global::Trigger.None },
        { Capability.Interact, global::Trigger.Interact },
        { Capability.Action,   global::Trigger.Action },
        { Capability.Attack1,  global::Trigger.Attack1 },
        { Capability.Attack2,  global::Trigger.Attack2 },
        { Capability.Modifier, global::Trigger.Modifier },
        { Capability.Dash,     global::Trigger.Dash },
    };

    public static readonly Dictionary<Trigger, Capability> Capabilities = new()
    {
        { global::Trigger.None,       Capability.None },
        { global::Trigger.Interact,   Capability.Interact },
        { global::Trigger.Action,     Capability.Action },
        { global::Trigger.Attack1,    Capability.Attack1 },
        { global::Trigger.Attack2,    Capability.Attack2 },
        { global::Trigger.Modifier,   Capability.Modifier },
        { global::Trigger.Dash,       Capability.Dash },
    };
}



