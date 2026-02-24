


using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Spawner : Service
{
    public SpawnPoint owner;
    public SpawnerDefinition Definition;
}

public abstract class SpawnerDefinition : Definition
{
    public abstract SpawnerClass SpawnerClass           { get; }
    public SpawnerSelectionMode SelectionMode           { get; set; }
    
    public List<string> Actors                          { get; set; }
    public Dictionary<string, int> Weights              { get; set; }
    
    public int MaxActiveTotal                           { get; set; }
    public int TotalSpawnLimit                          { get; set; }
}

public class TimedSpawnerDefinition : SpawnerDefinition
{
    public override SpawnerClass SpawnerClass => SpawnerClass.Timed;
    
    public float SpawnInterval                          { get; set; }
    public float InitialDelay                           { get; set; }
    public bool SpawnImmediately                        { get; set; }
}

public class SingleSpawnerDefinition : SpawnerDefinition
{
    public override SpawnerClass SpawnerClass => SpawnerClass.Single;
    
    public bool RespawnOnDeath                          { get; set; }
    public float RespawnDelay                           { get; set; }
}

public class MultiSpawnerDefinition : SpawnerDefinition
{
    public override SpawnerClass SpawnerClass => SpawnerClass.Multi;
        
    public Dictionary<string, float> SpawnChance        { get; set; }
    public Dictionary<string, int> MaxActivePerActor    { get; set; }
    public Dictionary<string, int> TotalLimitPerActor   { get; set; }
}


public class TriggeredSpawnerDefinition : SpawnerDefinition
{
    public override SpawnerClass SpawnerClass => SpawnerClass.Triggered;
    
    public string TriggerEventName                      { get; set; }
    public bool ConsumesTrigger                         { get; set; }
}

public class ConditionSpawnerDefinition : SpawnerDefinition
{
    public override SpawnerClass SpawnerClass => SpawnerClass.Condition;
    
    public string ConditionExpression                   { get; set; }
    public float CheckInterval                          { get; set; }
}

public enum SpawnerClass
{
    Timed,
    Single,
    Multi,
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



public class TimedSpawner : Spawner, IServiceLoop
{
    public new TimedSpawnerDefinition Definition        { get; init; }

    public Dictionary<string, Actor> actors             = new();

    public TimedSpawner(TimedSpawnerDefinition definition)
    {
        Services.Lane.Register(this);

        Definition = definition;    }

    // ===============================================================================

    public void Loop()
    {
        Spawn();
        Cull();
    }

    // ===============================================================================
        // REWORK REQUIRED CREATED FOR TESTING
    void Spawn()
    {
        if (actors.Count < Definition.MaxActiveTotal)
        {  
            foreach( var actor in Definition.Actors)
            {
                var instance = Factories.CreateActor(actor, owner.Anchor.View.transform.position);
                actors.Add(actor, instance);
            }
        }
    }
        // REWORK REQUIRED
    void Cull()
    {
        List<string> remove = new();

        foreach (var (actor, instance) in actors)
        {
            if (instance is ILiving living && living.Dead)
            {
                remove.Add(actor);
            }
        }

        remove.ForEach(actor => actors.Remove(actor));
        remove.Clear();
    }

    // ===============================================================================


    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.Spawner;
}

[Definition]
public class Test : TimedSpawnerDefinition
{
    public Test()
    {
        Name = "Test";

        Actors = new()
        {
            nameof(Dummy)
        };

        MaxActiveTotal = 1;
    }
}