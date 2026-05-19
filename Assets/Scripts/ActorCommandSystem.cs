using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class CommandSystem : ActorService, IServiceTick
{

    readonly Dictionary<Trigger, Command> commandBuffer     = new();
    readonly Dictionary<Trigger, Command> pendingBuffer     = new();

    // -----------------------------------
   
    readonly List<CommandAPI> queue                         = new();


    // ===============================================================================

    public CommandSystem(IntentSystem intent) : base(intent.Owner)
    {
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
        Log.Debug(queue.Count());

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
        Log.Debug($"creating command trigger { request.Trigger }");
        var command = new Command()
        {
            Data            = new()
            {
                Trigger     = request.Trigger
            },
        };

        pendingBuffer.Add(command.Data.Trigger, command);
    }

    void ReleaseCommand(CommandAPI request)
    {
        if (commandBuffer.TryGetValue(request.Trigger, out var command))
        {
            command.Data.Released       = true;
            command.Data.FrameReleased  = Clock.FrameCount;
        }

        if (pendingBuffer.TryGetValue(request.Trigger, out var pending))
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
            .Where(command => !command.Data.Locked && command.Data.Released && (Clock.FrameCount - command.Data.FrameReleased > Config.Input.BUFFER_WINDOW_FRAMES))
            .ToList();

        foreach (var command in toRemove)
            pendingBuffer.Remove(command.Data.Trigger);

        return toRemove.Count > 0;
    }

    bool RemoveReleasedActiveCommands()
    {
        if (commandBuffer.Count == 0)
            return false;

        var toRemove = commandBuffer.Values
            .Where(command => !command.Data.Locked && command.Data.Released)
            .ToList();

        foreach (var command in toRemove)
            commandBuffer.Remove(command.Data.Trigger);

        return toRemove.Count > 0;
    }

    void ConsumeCommand(CommandAPI request)
    {   
        var command  = pendingBuffer.Values.Where(command => command.Data.Trigger == request.Command.Data.Trigger).OrderByDescending(command => command.Data.FrameCreated).First();

        RemoveCommand (command, pendingBuffer);
        PromoteCommand(command, commandBuffer);
    }

    void LockCommand(CommandAPI request)
    {
        commandBuffer.FirstOrDefault(entry => entry.Value.RuntimeId == request.Command.RuntimeId).Value.Lock();
    }

    void UnlockCommand(CommandAPI request)
    {
        var Test = commandBuffer.FirstOrDefault(entry => entry.Value.RuntimeId == request.Command.RuntimeId);
        Test.Value.Unlock();

        Debug.Log($"Unlock command {Test.Value.Data.Trigger}");
    }
    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleCommandRequest(Message<Request, CommandAPI> message)
    {
        Debug.Log($"Caught request { message.Payload.Request }");
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
        buffer.Remove(command.Data.Trigger);
    }

    void PromoteCommand(Command command, Dictionary<Trigger, Command> buffer)
    {
        buffer.Add(command.Data.Trigger, command);
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
    public CommandData Data                     { get; set; }

    public void Lock()   => Data.Locked = true;
    public void Unlock() => Data.Locked = false;
}

public class CommandData
{
    public Trigger Trigger                      { get; init; }
    public IntentSnapshot Intent                { get; init; }

    public int FrameCreated                     { get; set;  }
    public int FrameReleased                    { get; set;  }
    
    public bool Locked                          { get; set;  }
    public bool Released                        { get; set;  }

    public CommandData()
    {
        FrameCreated = Clock.FrameCount;
    }
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

