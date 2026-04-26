using System.Collections.Generic;
using System.Linq;



public class CommandSystem : ActorService, IServiceTick
{

    readonly Dictionary<Trigger, Command> commandBuffer  = new();
    readonly Dictionary<Trigger, Command> pendingBuffer  = new();

    // -----------------------------------
   
    readonly List<CommandAPI> queue                         = new();


    // ===============================================================================

    public CommandSystem(IntentSystem intent) : base(intent.Owner)
    {
        owner.Bus.Link.Local<Message<Request, CommandAPI>>(HandleCommandRequest);

        Broadcast();
        Enable();
    }

    // ===============================================================================
    
    public void Tick()
    {
        ProcessQueue();
        MonitorBuffers();
    }

    void ProcessQueue()
    {
        foreach(var request in queue)
        {
           Process(request); 
        }

        queue.Clear();
    }

    void Process(CommandAPI command)
    {
        switch(command.Request)
        {
            case Request.Create:    CreateCommand(command);     break;
            case Request.Release:   ReleaseCommand(command);    break;
            case Request.Consume:   ConsumeCommand(command);    break;
            case Request.Lock:      LockCommand(command);       break;
            case Request.Unlock:    UnlockCommand(command);     break;
        }
        Broadcast();
    }

    void MonitorBuffers()
    {
        if(RemoveExpiredBufferCommands())
            Broadcast();
    }

    // ===============================================================================

    void CreateCommand(CommandAPI request)
    {
        var command = new Command()
        {
            Data            = new()
            {
                Trigger  = IntentMap.Triggers[request.Capability],
            },
            Configuration   = request.Configuration,
        };

        pendingBuffer.Add(command.Data.Trigger, command);
    }

    void ReleaseCommand(CommandAPI request)
    {
        if (commandBuffer.TryGetValue(IntentMap.Triggers[request.Capability], out var command))
        {
            command.Data.Released       = true;
            command.Data.FrameReleased  = Clock.FrameCount;
        }

        if (pendingBuffer.TryGetValue(IntentMap.Triggers[request.Capability], out var pending))
        {
            pending.Data.Released       = true;
            pending.Data.FrameReleased  = Clock.FrameCount;
        }
    }

    bool RemoveExpiredBufferCommands()
    {
        if (pendingBuffer.Count == 0) 
            return false;

        var toRemove = pendingBuffer.Values
            .Where(command => !command.Locked && command.Data.Released && command.Data.FrameReleased > Config.Input.BUFFER_WINDOW_FRAMES)
            .ToList();

        foreach (var command in toRemove)
            pendingBuffer.Remove(command.Data.Trigger);

        return toRemove.Count > 0;
    }

    void ConsumeCommand(CommandAPI request)
    {   
        var command  = pendingBuffer.Values.Where(command => command.Data.Trigger == request.Command.Data.Trigger).OrderByDescending(command => command.Data.FrameCreated).First();

        RemoveCommand(command, pendingBuffer);
        PromoteCommand(command, commandBuffer);
    }

    void LockCommand(CommandAPI request)
    {
        commandBuffer.FirstOrDefault(entry => entry.Value.RuntimeId == request.Command.RuntimeId).Value.Lock();
    }

    void UnlockCommand(CommandAPI request)
    {
        commandBuffer.FirstOrDefault(entry => entry.Value.RuntimeId == request.Command.RuntimeId).Value.Unlock();
    }
    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleCommandRequest(Message<Request, CommandAPI> message)
    {
        queue.Append(message.Payload);
    }

    void Broadcast() => owner.Bus.Emit.Local(new CommandPipelinesEvent(Snapshot.ReadOnly(commandBuffer), Snapshot.ReadOnly(pendingBuffer)));

    // ===============================================================================
    //  Helpers
    // ===============================================================================

    void RemoveCommand(Command command, Dictionary<Trigger, Command> buffer)
    {
        buffer.Remove(command.Data.Trigger);
    }

    void PromoteCommand(Command command, Dictionary<Trigger, Command> buffer)
    {
        buffer.Add(command.Data.Trigger, command);
    }

    // ===============================================================================

    public UpdatePriority Priority      => ServiceUpdatePriority.CommandSystem;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public class CommandAPI : API
{
    public Capability Capability                { get; init; } 
    public Command Command                      { get; set;  }

    public CommandConfiguration Configuration   { get; init; } = new();
}

public class CommandConfiguration
{
    public bool Instance                        { get; init; }
}

public class Command : Instance
{
    public CommandData Data                     { get; set; }
    public CommandConfiguration Configuration   { get; set; }

    public void Lock()   => Data.Locked = true;
    public void Unlock() => Data.Locked = false;

    public bool Locked   => Data.Locked;
}

public class CommandData
{
    public Trigger Trigger                      { get; init; }
    public IntentSnapshot Intent                { get; init; }

    public int FrameCreated                     { get; set;  }
    public int FrameReleased                    { get; set;  }
    
    public bool Locked;
    public bool Released                        { get; set;  }

    public CommandData()
    {
        FrameCreated = Clock.FrameCount;
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Maps                                                  
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

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
    public IReadOnlyDictionary<Trigger, Command> Active { get; init; }
    public IReadOnlyDictionary<Trigger, Command> Buffer { get; init; }

    public CommandPipelinesEvent(IReadOnlyDictionary<Trigger, Command> active, IReadOnlyDictionary<Trigger, Command> buffer)
    {
        Active = active;
        Buffer = buffer;
    }
}

