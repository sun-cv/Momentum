using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;




public class Command : Instance
{
    public Capability  Action   { get; init; }
    public InputButton Button   { get; init; }
    public int FrameCreated     { get; init; }

    public bool locked;

    public Command(Capability action, InputButton button)
    {
        Action       = action;
        Button       = button;
        FrameCreated = Clock.FrameCount;
    }

    public void Lock()   => locked = true;
    public void Unlock() => locked = false;

    public bool Locked   => locked;
}



public class CommandSystem
{
    IntentSystem intent;

    Dictionary<Capability, Command> active = new();
    Dictionary<Capability, Command> buffer = new();


    public void Initialize(IntentSystem intent)
    {
        this.intent = intent;
        EventBus<CommandRequest>.Subscribe(HandleCommandRequest);
        PublishUpdate();
    }

    public void Tick()
    {
        MonitorEdgePress();
        MonitorEdgeRelease();
    }

    void MonitorEdgePress()
    {
        if (CreatePressedCommands())
            PublishUpdate();
    }

    void MonitorEdgeRelease()
    {
        if (RemoveExpiredBufferCommands() || RemoveReleasedActiveCommands())
            PublishUpdate();
    }

    bool CreatePressedCommands()
    {
        var toCreate = intent.Input.ActiveButtons.Where(button => button.pressedThisFrame).ToList();

        foreach (var button in toCreate)
        {
            var capability = IntentMap.Input[button.Input];

            if (buffer.ContainsKey(capability))
                buffer.Remove(capability);

            buffer[capability] = new(capability, button);        
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

        PublishUpdate();
    }

    void ConsumeCommand(Command command)
    {   
        var instance  = buffer.Values.Where(instance => instance.Action == command.Action).OrderBy(instance => instance.Button.pressedframeCount.CurrentFrame).First();

        buffer.Remove(command.Action);
        active[command.Action] = instance;
    }
    void LockCommand(Command command)    => active.FirstOrDefault(entry => entry.Value.RuntimeID == command.RuntimeID).Value.Lock();
    void UnlockCommand(Command command)  => active.FirstOrDefault(entry => entry.Value.RuntimeID == command.RuntimeID).Value.Unlock();

    void PublishUpdate() => OnEvent<CommandPublish>(new(Guid.NewGuid(), Publish.Changed, new(){ Active = Snapshot.ReadOnly(active), Buffer = Snapshot.ReadOnly(buffer) }));

    void OnEvent<T>(T evt) where T : IEvent     => EventBus<T>.Raise(evt);
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
    public static readonly Dictionary<InputIntent, Capability> Input = new()
    {
        { InputIntent.None,     Capability.None },
        { InputIntent.Interact, Capability.Interact },
        { InputIntent.Action,   Capability.Action },
        { InputIntent.Attack1,  Capability.Attack1 },
        { InputIntent.Attack2,  Capability.Attack2 },
        { InputIntent.Modifier, Capability.Modifier },
        { InputIntent.Dash,     Capability.Dash },
    };

    public static readonly Dictionary<Capability, InputIntent> Capabilities = new()
    {
        { Capability.None,       InputIntent.None },
        { Capability.Interact,   InputIntent.Interact },
        { Capability.Action,     InputIntent.Action },
        { Capability.Attack1,    InputIntent.Attack1 },
        { Capability.Attack2,    InputIntent.Attack2 },
        { Capability.Modifier,   InputIntent.Modifier },
        { Capability.Dash,       InputIntent.Dash },
    };
}
