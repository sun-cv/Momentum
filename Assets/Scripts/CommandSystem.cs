using System.Collections.Generic;
using System.Linq;



public class CommandSystem : ActorService, IServiceTick
{
    readonly IntentSystem intent;

    // -----------------------------------

    readonly Dictionary<Capability, Command> activeBuffer   = new();
    readonly Dictionary<Capability, Command> inputBuffer    = new();

    
    // ===============================================================================

    public CommandSystem(IntentSystem intent) : base(intent.Owner)
    {
        this.intent = intent;

        owner.Bus.Link.Local<CommandEvent>(HandleCommandRequest);
        Broadcast();
    }

    // ===============================================================================
    
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

    // ===============================================================================

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

            if (inputBuffer.ContainsKey(capability))
                inputBuffer.Remove(capability);

            inputBuffer[capability] = command;       
        }
        return toCreate.Count > 0;
    }

    bool RemoveExpiredBufferCommands()
    {
        if (inputBuffer.Count == 0) return false;

    var toRemove = inputBuffer.Values
        .Where(command => !command.Locked && command.Button.released && command.Button.releasedframeCount.CurrentFrame > Config.Input.BUFFER_WINDOW_FRAMES)
        .ToList();

        foreach (var command in toRemove)
            inputBuffer.Remove(command.Action);

        return toRemove.Count > 0;
    }

    bool RemoveReleasedActiveCommands()
    {
        if (activeBuffer.Count == 0) return false;

        var toRemove = activeBuffer.Values.Where(command => !command.Locked && command.Button.released).ToList();

        foreach (var command in toRemove)
            activeBuffer.Remove(command.Action);

        return toRemove.Count > 0;
    }

    void ConsumeCommand(Command command)
    {   
        var instance  = inputBuffer.Values.Where(instance => instance.Action == command.Action).OrderBy(instance => instance.Button.pressedframeCount.CurrentFrame).First();

        inputBuffer.Remove(command.Action);
        activeBuffer[command.Action] = instance;
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleCommandRequest(CommandEvent instance)
    {
        switch (instance.Type)
        {
            case Request.Consume:
                ConsumeCommand(instance.Command);
                break;
            case Request.Lock:
                LockCommand(instance.Command);
                break;
            case Request.Unlock:
                UnlockCommand(instance.Command);
                break;
        }

        Broadcast();
    }

        // ===================================
        //  Emitters
        // ===================================

    void Broadcast() => owner.Bus.Emit.Local(new CommandPipelinesEvent(Snapshot.ReadOnly(activeBuffer), Snapshot.ReadOnly(inputBuffer)));

    // ===============================================================================
    //  Helpers
    // ===============================================================================

    void LockCommand(Command command)    => activeBuffer.FirstOrDefault(entry => entry.Value.RuntimeId == command.RuntimeId).Value.Lock();
    void UnlockCommand(Command command)  => activeBuffer.FirstOrDefault(entry => entry.Value.RuntimeId == command.RuntimeId).Value.Unlock();

    // ===============================================================================

    public UpdatePriority Priority      => ServiceUpdatePriority.CommandSystem;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

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

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Maps                                                  
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public static partial class IntentMap
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



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct CommandEvent : IMessage
{
    public Command Command  { get; init; }
    public Request Type     { get; init; }

    public CommandEvent(Request type, Command command)
    {
        Command = command;
        Type    = type;
    }
}

public readonly struct CommandPipelinesEvent : IMessage
{
    public IReadOnlyDictionary<Capability, Command> Active { get; init; }
    public IReadOnlyDictionary<Capability, Command> Buffer { get; init; }

    public CommandPipelinesEvent(IReadOnlyDictionary<Capability, Command> active, IReadOnlyDictionary<Capability, Command> buffer)
    {
        Active = active;
        Buffer = buffer;
    }
}

