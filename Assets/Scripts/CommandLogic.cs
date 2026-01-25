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

        LinkLocal<CommandRequest>(HandleCommandRequest);
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

    void HandleCommandRequest(CommandRequest evt)
    {
        switch (evt.Action)
        {
            case Request.Consume:
                ConsumeCommand(evt.Payload.Command);
                break;
            case Request.Lock:
                LockCommand(evt.Payload.Command);
                break;
            case Request.Unlock:
                UnlockCommand(evt.Payload.Command);
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

    void Broadcast() => EmitLocal<CommandPublish>(new(Guid.NewGuid(), Publish.Changed, new(){ Active = Snapshot.ReadOnly(active), Buffer = Snapshot.ReadOnly(buffer) }));

    void LinkLocal <T>(Action<T> handler) where T : IEvent  => owner.Bus.Subscribe(handler);
    void EmitLocal <T>(T evt) where T : IEvent              => owner.Bus.Raise(evt);
}

public readonly struct CommandRequestPayload
{
    public Command Command { get; init; }
}

public readonly struct CommandStatePayload
{
    public IReadOnlyDictionary<Capability, Command> Active { get; init; }
    public IReadOnlyDictionary<Capability, Command> Buffer { get; init; }
}

public readonly struct CommandRequest : ISystemEvent
{
    public Guid Id                          { get; }
    public Request Action                   { get; }
    public CommandRequestPayload Payload    { get; }

    public CommandRequest(Guid id, Request action, CommandRequestPayload payload)
    {
        Id      = id;
        Action  = action;
        Payload = payload;
    }
}

public readonly struct CommandResponse : ISystemEvent
{
    public Guid Id                          { get; }
    public Response Response                { get; }

    public CommandResponse(Guid id, Response response)
    {
        Id       = id;
        Response = response;
    }
}

public readonly struct CommandPublish : ISystemEvent
{
    public Guid Id                          { get; }
    public Publish Event                    { get; }
    public CommandStatePayload Payload      { get; }

    public CommandPublish(Guid id, Publish evt, CommandStatePayload payload)
    {
        Id      = id;
        Event   = evt;
        Payload = payload;
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
