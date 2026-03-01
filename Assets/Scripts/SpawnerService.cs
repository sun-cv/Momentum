using System;
using System.Collections.Generic;
using System.Linq;



public class SpawnerService : RegisteredService, IServiceLoop
{

    public static bool ShowDebugGizmos                              = true;

        // -----------------------------------

    readonly List<Spawner>          spawners = new();
    readonly List<SpawnerRequest>   requests = new();

    // ===============================================================================

    public SpawnerService()
    {
        Link.Global<Message<Request, SpawnerEvent>>(HandleSpawnerEventRequest);
    }

    // ===============================================================================

    public void Loop()
    {
        ProcessRequests();
    }

    // ===============================================================================

    void ProcessRequests()
    {
        foreach ( var request in requests)
        {
            ResolveRequest(request);
        }

        requests.Clear();
    }


    void ResolveRequest(SpawnerRequest request)
    {
        var spawnPoint = request.SpawnPoint;

        switch(request.Action)
        {
            case Request.Create: 
                    CreateSpawnPoint(spawnPoint);
                break;
            case Request.Destroy:
                    DestroySpawnPoint(spawnPoint);
                break;
        }
    }

    void CreateSpawnPoint(SpawnPoint point)
    {
        var definition  = Definitions.Get<SpawnerDefinition>(point.Name);
        var spawner     = CreateSpawner(definition);
        
        spawner.owner   = point;
        point.Spawner   = spawner;

        Register(spawner);
    }

    Spawner CreateSpawner(SpawnerDefinition definition)
    {
        return SpawnerFactory[definition.SpawnerClass](definition);
    }

    void DestroySpawnPoint(SpawnPoint spawnPoint)
    {
        Deregister(spawnPoint.Spawner);
    }

    void Register(Spawner spawner)
    {
        spawners.Add(spawner);
    }

    void Deregister(Spawner spawner)
    {
        spawners.Remove(spawner);
    }
     
    void Clear()
    {
        spawners.Clear();
    }

    Spawner GetSpawner(string name)
    {
        return spawners.FirstOrDefault(spawner => spawner.Definition.Name == name);
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleSpawnerEventRequest(Message<Request, SpawnerEvent> message)
    {   
        requests.Add(new() { Action = message.Action, SpawnPoint = message.Payload.SpawnPoint });
    }

    // ===============================================================================

    public Dictionary<SpawnerClass, Func<SpawnerDefinition, Spawner>> SpawnerFactory = new()
    {
        { SpawnerClass.Timed,   (definition) => new TimedSpawner ((TimedSpawnerDefinition)definition) },
        { SpawnerClass.Single,  (definition) => new SingleSpawner((SingleSpawnerDefinition)definition)}
    };

    // ===============================================================================

    readonly Logger Log = new(LogSystem.Spawners, LogLevel.Debug);

    readonly EventBinding<Message<Request, TeleportEvent>> binding;

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
        
        spawners.Clear();

        Link.UnsubscribeGlobal(binding);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.SpawnerService;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public struct SpawnerRequest
{
    public Request Action           { get; set; }
    public SpawnPoint SpawnPoint    { get; set; }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public readonly struct SpawnerEvent
{
    public readonly SpawnPoint SpawnPoint  { get; }

    public SpawnerEvent(SpawnPoint spawnPoint)
    {
        SpawnPoint = spawnPoint;
    }
}