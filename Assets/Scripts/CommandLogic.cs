using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;




public class Command : Instance
{
    public InputIntent Input    { get; init; }
    public InputButton Button   { get; init; }

    public bool locked;

    public Command(InputIntent input, InputButton button)
    {
        Input  = input;
        Button = button;
    }

    public void Lock()   => locked = true;
    public void Unlock() => locked = false;

    public bool Locked   => locked;
}



public class CommandSystem : RegisteredService, IServiceTick
{

    InputRouter router;

    Dictionary<InputIntent, Command> active = new();
    Dictionary<InputIntent, Command> buffer = new();


    public override void Initialize()
    {
        router       = Services.Get<InputRouter>();
        Services.RegisterTick(this);

        EventBus<CommandRequest>.Subscribe(HandleCommandRequest);
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
            buffer[button.Input] = new(button.Input, button);

        return toCreate.Count > 0;
    }

    bool RemoveReleasedCommands(Dictionary<InputIntent, Command> commands)
    {
        if (commands.Count == 0) return false;

        var toRemove = commands.Values.Where(command => !command.Locked && command.Button.releasedThisFrame).ToList();

        foreach (var command in toRemove)
            commands.Remove(command.Input);

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
        var instance  = buffer.Values.Where(instance => instance.Input == command.Input).OrderBy(instance => instance.Button.pressedframeCount.CurrentFrame).First();

        buffer.Remove(command.Input);
        active[command.Input] = instance;
    }
    void LockCommand(Command command)    => active[command.Input].Lock();
    void UnlockCommand(Command command)  => active[command.Input].Unlock();

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
    public IReadOnlyDictionary<InputIntent, Command> Active { get; init; }
    public IReadOnlyDictionary<InputIntent, Command> Buffer { get; init; }
}

public readonly struct CommandRequest : IEventRequest
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

public readonly struct CommandResponse : IEventResponse
{
    public Guid Id                          { get; }
    public Response Response                { get; }

    public CommandResponse(Guid id, Response response)
    {
        Id       = id;
        Response = response;
    }
}

public readonly struct CommandPublish : IEventPublish
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