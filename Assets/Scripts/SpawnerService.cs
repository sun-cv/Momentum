using System;
using System.Collections.Generic;
using System.Linq;



public class SpawnerService : RegisteredService, IServiceLoop
{

    readonly List<Spawner>  spawners  = new();

    // ===============================================================================

    public SpawnerService()
    {
        
    }

    // ===============================================================================

    public void Loop()
    {
    }

    // ===============================================================================

    public Spawner CreateSpawner(SpawnerDefinition definition)
    {
        return SpawnerFactory[definition.SpawnerClass](definition);
    }

    // ===============================================================================

    public void Register(SpawnPoint point)
    {
        var definition  = Definitions.Get<SpawnerDefinition>(point.Name);
        var spawner     = CreateSpawner(definition);
        spawner.owner   = point;

        spawners.Add(spawner);
    }


    public void Register(Spawner spawner)
    {
        spawners.Add(spawner);
    }

    public void Deregister(Spawner spawner)
    {
        spawners.Remove(spawner);
    }
     
    public void Clear()
    {
        spawners.Clear();
    }

    public Spawner GetSpawner(string name)
    {
        return spawners.FirstOrDefault(spawner => spawner.Definition.Name == name);
    }

    // ===============================================================================

    public Dictionary<SpawnerClass, Func<SpawnerDefinition, Spawner>> SpawnerFactory = new()
    {
        { SpawnerClass.Timed, (definition) => new TimedSpawner((TimedSpawnerDefinition)definition) }
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

    public UpdatePriority Priority => ServiceUpdatePriority.TeleportService;
}
