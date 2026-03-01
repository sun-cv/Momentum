using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public abstract class Spawner : Service
{
    public SpawnPoint owner;
    public SpawnerDefinition Definition;
}



public class SingleSpawner : Spawner, IServiceStep
{
    public new SingleSpawnerDefinition Definition        { get; init; }

        // -----------------------------------

    public Actor        actor;
    public ActorEntry   entry;
    public ClockTimer   timer;

        // -----------------------------------

    bool HasSpawned;

    // ===============================================================================

    public SingleSpawner(SingleSpawnerDefinition definition)
    {
        Services.Lane.Register(this);

        Definition = definition;    
        
        InitializeTimer();
    }

    // ===============================================================================

    public void Step()
    {
        Cull();

        if (CanSpawnActor())
            Spawn();
    }

    // ===============================================================================

    void Spawn()
    {
        entry           = Definition.Actors.First();
        var factory     = Factories.Get<IActorFactory>(entry.Name);
        var instance    = factory.Spawn(owner.Anchor.View.transform.position);

        actor           = instance;
        HasSpawned      = true;
    }

    void Cull()
    {
        if (actor == null)
            return;

        if (actor is ILiving entity && entity.Dead)
        {
            actor = null;
            timer.Restart(entry.RespawnDelay);
        }
    }

    void InitializeTimer()
    {
        entry        = Definition.Actors.First();
        float delay  = entry.InitialDelay > 0 ? entry.InitialDelay : Definition.InitialDelay;
        timer        = new(delay);

        timer.Start();
    }

    // ===============================================================================
    //  Predicates
    // ===============================================================================

    bool CanSpawnActor()
    {
        if (!timer.IsFinished)
            return false;

        if (actor != null)
            return false;

        if (HasSpawned && !entry.RespawnOnDeath)
            return false;

        return true;
    }

    // ===============================================================================


    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.Spawner;
}


public class TimedSpawner : Spawner, IServiceStep
{
    public new TimedSpawnerDefinition Definition        { get; init; }

    public Dictionary<string, List<Actor>> actors       = new();
    public Dictionary<string, ClockTimer> timers        = new();

    public TimedSpawner(TimedSpawnerDefinition definition)
    {
        Services.Lane.Register(this);

        Definition = definition;    
        
        InitializeLists();
        InitializeTimers();
    }

    // ===============================================================================

    public void Step()
    {
        CullActors();
        SpawnActors();
    }

    // ===============================================================================

    void SpawnActors()
    {
        foreach(var actor in Definition.Actors)
        {

            if (CanSpawnActor(actor))
                Spawn(actor);
        }
    }

    void Spawn(ActorEntry actor)
    {
        if (!TryGetSpawnPoint(actor, out var point))
            return;
        
        var factory  = Factories.Get<IActorFactory>(actor.Name);
        var instance = factory.Spawn(point);

        actors[actor.Name].Add(instance);

        float interval      = actor.SpawnInterval > 0 ? actor.SpawnInterval : Definition.SpawnInterval;
        timers[actor.Name]  = new(interval);
        timers[actor.Name].Start();
    }

    void CullActors()
    {
        foreach (var (actor, list) in actors)
        {
            Cull(actor, list);
        }
    }

    void Cull(string actor, List<Actor> list)
    {
        int removed = list.RemoveAll(instance => instance is ILiving entity && entity.Dead);

        if (removed > 0)
        {
            var respawnDelay = Definition.Actors.First(entry => entry.Name == actor).RespawnDelay;
            var delay        = respawnDelay > 0 ? respawnDelay : Definition.RespawnDelay;
            timers[actor].Restart(delay);
        }
    }

    bool TryGetSpawnPoint(ActorEntry actor, out Vector2 point)
    {
        var bounds      = owner.Area.bounds;
        int attempts    = 10;
        var mask        = ~(1 << owner.Area.gameObject.layer);


        for (int i = 0; i < attempts; i++)
        {
            var candidate = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );

            if (!owner.Area.OverlapPoint(candidate))
                continue;

            if (Physics2D.OverlapCircle(candidate, actor.SpawnRadius, mask) != null)
                continue;

            point = candidate;
            return true;
        }

        point = default;
        return false;
    }

    void InitializeLists()
    {
        Definition.Actors.ForEach(actor => { actors[actor.Name] = new(); });
    }

    void InitializeTimers()
    {
        foreach (var actor in Definition.Actors)
        {
            float delay  = actor.InitialDelay > 0 ? actor.InitialDelay : Definition.InitialDelay;
            timers[actor.Name] = new(delay);
            timers[actor.Name].Start();
        }
    }

    // ===============================================================================
    //  Predicates
    // ===============================================================================

    bool CanSpawnActor(ActorEntry actor)
    {
        if (!timers[actor.Name].IsFinished)
            return false;

        if (actor.TotalLimit > 0 && actors[actor.Name].Count >= actor.TotalLimit)
            return false;

        if (actor.MaxActive > 0 && actors[actor.Name].Count >= actor.MaxActive)
            return false;

        if (Definition.MaxActive > 0 && actors.Values.Sum(list => list.Count) >= Definition.MaxActive)
            return false;

        return true;
    }

    // ===============================================================================


    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.Spawner;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public class ActorEntry
{
    public string Name                          { get; set; }
    public int MaxActive                        { get; set; }
    public int TotalLimit                       { get; set; }
    public int Weight                           { get; set; }
    public float InitialDelay                   { get; set; }
    public float SpawnInterval                  { get; set; }
    public float SpawnRadius                    { get; set; }
    public float SpawnChance                    { get; set; }
    public float RespawnDelay                   { get; set; }
    public bool RespawnOnDeath                  { get; set; }
}


public abstract class SpawnerDefinition : Definition
{
    public abstract SpawnerClass SpawnerClass   { get; }
    public SpawnerSelectionMode SelectionMode   { get; set; }

    public List<ActorEntry> Actors              { get; set; }

    public int MaxActive                        { get; set; }
    public int SpawnLimit                       { get; set; }

    public float InitialDelay                   { get; set; }
    public float RespawnDelay                   { get; set; }
    public float SpawnInterval                  { get; set; }
    public bool SpawnImmediately                { get; set; }
}

public class SingleSpawnerDefinition : SpawnerDefinition
{
    public override SpawnerClass SpawnerClass => SpawnerClass.Single;
}

public class TimedSpawnerDefinition : SpawnerDefinition
{
    public override SpawnerClass SpawnerClass => SpawnerClass.Timed;
}

public class TriggeredSpawnerDefinition : SpawnerDefinition
{
    public override SpawnerClass SpawnerClass   => SpawnerClass.Triggered;
    public string TriggerEvent                  { get; set; }
    public bool ConsumesTrigger                 { get; set; }
}

public class ConditionSpawnerDefinition : SpawnerDefinition
{
    public override SpawnerClass SpawnerClass   => SpawnerClass.Condition;
    public string Condition                     { get; set; }
    public float CheckInterval                  { get; set; }
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Enums                                                 
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum SpawnerClass
{
    Timed,
    Single,
    Triggered,
    Condition
}

public enum SpawnerSelectionMode
{
    Random,
    Sequential,
    Weighted,
    Wave,
}



[Definition]
public class TimedSpawnerTest : TimedSpawnerDefinition
{
    public TimedSpawnerTest()
    {
        Name                        = "TimedSpawnerTest";

        Actors                      = new()
        {   
            new()
            {
                Name = nameof(Dummy),
                InitialDelay        = 1,
                SpawnInterval       = 2,
                SpawnRadius         = 1f
            },
        };  

        MaxActive                   = 10;
        SpawnInterval               = 2f;
        InitialDelay                = 2f;
    }
}

[Definition]
public class SpawnDummy : SingleSpawnerDefinition
{
    public SpawnDummy()
    {
        Name                        = "SpawnDummy";

        Actors                      = new()
        {   
            new()
            {
                Name                = nameof(Dummy),
                InitialDelay        = 2,
                RespawnDelay        = 4,
                RespawnOnDeath      = true,
            },
        };  

        MaxActive                   = 10;
        SpawnInterval               = 2f;
        InitialDelay                = 2f;
    }
}           

[Definition]
public class SpawnMovableDummy : SingleSpawnerDefinition
{
    public SpawnMovableDummy()
    {
        Name                        = "SpawnMovableDummy";

        Actors                      = new()
        {   
            new()
            {
                Name                = nameof(MovableDummy),
                InitialDelay        = 2,
                RespawnDelay        = 4,
                RespawnOnDeath      = true,
            },
        };  

        MaxActive                   = 10;
        SpawnInterval               = 2f;
        InitialDelay                = 2f;
    }
}           


