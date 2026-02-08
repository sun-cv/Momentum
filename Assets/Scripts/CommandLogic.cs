using System;
using System.Collections.Generic;
using System.Linq;




public class Command : Instance
{
    public Capability           Action  { get; init; }
    public InputButton          Button  { get; init; }
    public InputIntentSnapshot  Intent  { get; init; }

    public int FrameCreated     { get; init; }

    public bool locked;

    public Command()
    {
        FrameCreated = Clock.FrameCount;
    }

    public void Lock()   => locked = true;
    public void Unlock() => locked = false;

    public bool Locked   => locked;
}




public class CommandSystem
{
    Actor owner;
    IntentSystem intent;

    Dictionary<Capability, Command> active = new();
    Dictionary<Capability, Command> buffer = new();


    public void Initialize(IntentSystem intent)
    {
        this.intent = intent;
        this.owner  = intent.Owner;

        owner.Emit.Link.Local<Request, CommandEvent>(HandleCommandRequest);
        Broadcast();
    }

    public void Tick()
    {
        MonitorEdgePress();
        MonitorEdgeRelease();
    }

    void MonitorEdgePress()
    {
        if (CreatePressedCommands())
            Broadcast();
    }

    void MonitorEdgeRelease()
    {
        if (RemoveExpiredBufferCommands() || RemoveReleasedActiveCommands())
            Broadcast();
    }

    bool CreatePressedCommands()
    {
        var toCreate = intent.InputRouter.ActiveButtons.Where(button => button.pressedThisFrame).ToList();

        foreach (var button in toCreate)
        {
            var capability  = IntentMap.Input[button.Input];
            var command     = new Command()
            {
                Action = capability,
                Button = button,
                Intent = intent.Input.Snapshot()
            };

            if (buffer.ContainsKey(capability))
                buffer.Remove(capability);

            buffer[capability] = command;       
        }
        return toCreate.Count > 0;
    }

    bool RemoveExpiredBufferCommands()
    {
        if (buffer.Count == 0) return false;

    var toRemove = buffer.Values
        .Where(command => !command.Locked && command.Button.released && command.Button.releasedframeCount.CurrentFrame > Config.Input.BUFFER_WINDOW_FRAMES)
        .ToList();
        foreach (var command in toRemove)
            buffer.Remove(command.Action);

        return toRemove.Count > 0;
    }

    bool RemoveReleasedActiveCommands()
    {
        if (active.Count == 0) return false;

        var toRemove = active.Values.Where(command => !command.Locked && command.Button.released).ToList();

        foreach (var command in toRemove)
            active.Remove(command.Action);

        return toRemove.Count > 0;
    }

    void HandleCommandRequest(Message<Request, CommandEvent> message)
    {
        switch (message.Action)
        {
            case Request.Consume:
                ConsumeCommand(message.Payload.Command);
                break;
            case Request.Lock:
                LockCommand(message.Payload.Command);
                break;
            case Request.Unlock:
                UnlockCommand(message.Payload.Command);
                break;
        }

        Broadcast();
    }

    void ConsumeCommand(Command command)
    {   
        var instance  = buffer.Values.Where(instance => instance.Action == command.Action).OrderBy(instance => instance.Button.pressedframeCount.CurrentFrame).First();

        buffer.Remove(command.Action);
        active[command.Action] = instance;
    }
    void LockCommand(Command command)    => active.FirstOrDefault(entry => entry.Value.RuntimeID == command.RuntimeID).Value.Lock();
    void UnlockCommand(Command command)  => active.FirstOrDefault(entry => entry.Value.RuntimeID == command.RuntimeID).Value.Unlock();

    void Broadcast() => owner.Emit.Local(Guid.NewGuid(), Publish.Changed, new CommandPipelinesEvent(Snapshot.ReadOnly(active), Snapshot.ReadOnly(buffer)));
}




public readonly struct CommandEvent
{
    public Command Command { get; init; }

    public CommandEvent(Command command)
    {
        Command = command;
    }
}


public readonly struct CommandPipelinesEvent
{
    public IReadOnlyDictionary<Capability, Command> Active { get; init; }
    public IReadOnlyDictionary<Capability, Command> Buffer { get; init; }

    public CommandPipelinesEvent(IReadOnlyDictionary<Capability, Command> active, IReadOnlyDictionary<Capability, Command> buffer)
    {
        Active = active;
        Buffer = buffer;
    }
}




public static class IntentMap
{
    public static readonly Dictionary<PlayerAction, Capability> Input = new()
    {
        { PlayerAction.None,     Capability.None },
        { PlayerAction.Interact, Capability.Interact },
        { PlayerAction.Action,   Capability.Action },
        { PlayerAction.Attack1,  Capability.Attack1 },
        { PlayerAction.Attack2,  Capability.Attack2 },
        { PlayerAction.Modifier, Capability.Modifier },
        { PlayerAction.Dash,     Capability.Dash },
    };

    public static readonly Dictionary<Capability, PlayerAction> Capabilities = new()
    {
        { Capability.None,       PlayerAction.None },
        { Capability.Interact,   PlayerAction.Interact },
        { Capability.Action,     PlayerAction.Action },
        { Capability.Attack1,    PlayerAction.Attack1 },
        { Capability.Attack2,    PlayerAction.Attack2 },
        { Capability.Modifier,   PlayerAction.Modifier },
        { Capability.Dash,       PlayerAction.Dash },
    };
}
