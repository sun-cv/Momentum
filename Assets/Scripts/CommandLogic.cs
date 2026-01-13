using System;
using System.Collections.Generic;
using System.Linq;




public class Command : Instance
{
    public Capability  Action   { get; init; }
    public InputButton Button   { get; init; }

    public bool locked;

    public Command(Capability action, InputButton button)
    {
        Action = action;
        Button = button;
    }

    public void Lock()   => locked = true;
    public void Unlock() => locked = false;

    public bool Locked   => locked;
}



public class CommandSystem : RegisteredService, IServiceTick
{

    InputRouter router;

    Dictionary<Capability, Command> active = new();
    Dictionary<Capability, Command> buffer = new();


    public override void Initialize()
    {
        router       = Services.Get<InputRouter>();
        Services.RegisterTick(this);

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
        if (RemoveReleasedCommands(buffer) | RemoveReleasedCommands(active))
            PublishUpdate();
    }

    bool CreatePressedCommands()
    {
        var toCreate = router.ActiveButtons.Where(button => button.pressedThisFrame == true).ToList();

        foreach (var button in toCreate)
            buffer[IntentMap.Input[button.Input]] = new(IntentMap.Input[button.Input], button);

        return toCreate.Count > 0;
    }

    bool RemoveReleasedCommands(Dictionary<Capability, Command> commands)
    {
        if (commands.Count == 0) return false;

        var toRemove = commands.Values.Where(command => !command.Locked && command.Button.releasedThisFrame).ToList();

        foreach (var command in toRemove)
            commands.Remove(command.Action);

        return toRemove.Count > 0;
    }


    void HandleCommandRequest(CommandRequest evt)
    {
        switch (evt.Action)
        {
            case CommandAction.Consume:
                ConsumeCommand(evt.Payload.Command);
                break;
            case CommandAction.Lock:
                LockCommand(evt.Payload.Command);
                break;
            case CommandAction.Unlock:
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
    void LockCommand(Command command)    => active[command.Action].Lock();
    void UnlockCommand(Command command)  => active[command.Action].Unlock();

    void PublishUpdate() => OnEvent<CommandPublish>(new(Guid.NewGuid(), Publish.Changed, new(){ Active = Snapshot.ReadOnly(active), Buffer = Snapshot.ReadOnly(buffer) }));


    void OnEvent<T>(T evt) where T : IEvent     => EventBus<T>.Raise(evt);
    public UpdatePriority Priority              => ServiceUpdatePriority.CommandSystem;
}



public enum CommandAction
{
    Consume,
    Lock,
    Unlock
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
    public CommandAction Action             { get; }
    public CommandRequestPayload Payload    { get; }

    public CommandRequest(Guid id, CommandAction action, CommandRequestPayload payload)
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
