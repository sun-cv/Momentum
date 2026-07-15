using System.Collections.Generic;
using System.Linq;



public class CommandSystem : ActorService, IServiceTick
{

    readonly IntentSystem Intent;

    // -----------------------------------

    readonly Dictionary<Trigger, Command> commandBuffer     = new();
    readonly Dictionary<Trigger, Command> pendingBuffer     = new();

    // -----------------------------------
   
    readonly List<CommandAPI> queue                         = new();

    // ===============================================================================

    public CommandSystem(IntentSystem intent) : base(intent.Owner)
    {
        Intent = intent;

        owner.Bus.Link.Local<Message<Request, CommandAPI>>(HandleCommandRequest);

        Broadcast();
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
        var changed = false;

        changed |= RemoveExpiredBufferCommands();
        changed |= RemoveReleasedActiveCommands();

        if (changed)
            Broadcast();
    }

    // ===============================================================================

    void CreateCommand(CommandAPI request)
    {
        var command = new Command()
        {
            Trigger     = request.Trigger,
            Intent      = Intent.Snapshot(),
        };

        pendingBuffer[command.Trigger] = command;
    }

    void ReleaseCommand(CommandAPI request)
    {
        if (commandBuffer.TryGetValue(request.Trigger, out var command))
        {
            command.Released       = true;
            command.FrameReleased  = Clock.FrameCount;
        }

        if (pendingBuffer.TryGetValue(request.Trigger, out var pending))
        {
            pending.Released       = true;
            pending.FrameReleased  = Clock.FrameCount;
        }
    }

    bool RemoveExpiredBufferCommands()
    {
        if (pendingBuffer.Count == 0) 
            return false;

        var toRemove = pendingBuffer.Values
            .Where(command => !command.Locked && command.Released && (Clock.FrameCount - command.FrameReleased > Config.Input.BUFFER_WINDOW_FRAMES))
            .ToList();

        foreach (var command in toRemove)
            RemoveCommand(command, pendingBuffer);

        return toRemove.Count > 0;
    }

    bool RemoveReleasedActiveCommands()
    {
        if (commandBuffer.Count == 0)
            return false;

        var toRemove = commandBuffer.Values
            .Where(command => command.Released && !command.Locked)
            .ToList();

        foreach (var command in toRemove)
            RemoveCommand(command, commandBuffer);

        return toRemove.Count > 0;
    }

    void ConsumeCommand(CommandAPI request)
    {   
        var command  = pendingBuffer.Values.Where(command => command.Trigger == request.Command.Trigger).OrderByDescending(command => command.FrameCreated).First();

        RemoveCommand (command, pendingBuffer);
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
        queue.Add(message.Payload);
    }

    void Broadcast()
    {
        owner.Bus.Emit.Local(new CommandPipelinesEvent(Snapshot.ReadOnly(commandBuffer), Snapshot.ReadOnly(pendingBuffer)));
    }
    // ===============================================================================
    //  Helpers
    // ===============================================================================

    void RemoveCommand(Command command, Dictionary<Trigger, Command> buffer)
    {
        buffer.Remove(command.Trigger);
    }

    void PromoteCommand(Command command, Dictionary<Trigger, Command> buffer)
    {
        buffer[command.Trigger] = command;
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Input);

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
    public Trigger Trigger                      { get; set;  }
    public Command Command                      { get; set;  }
}

public class Command : Instance
{
    public Trigger Trigger                      { get; init; }
    public IntentSnapshot Intent                { get; init; }

    public int FrameCreated                     { get; set;  }
    public int FrameReleased                    { get; set;  }
    
    public bool Locked                          { get; set;  }
    public bool Released                        { get; set;  }

    public Command()
    {
        FrameCreated = Clock.FrameCount;
    }

    public void Lock()   => Locked = true;
    public void Unlock() => Locked = false;
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

