using System;
using System.Collections.Generic;





public class Stats : Service, IServiceTick
{
    protected readonly Dictionary<string, float> stats  = new();
    protected readonly StatsMediator mediator           = new();
    
    public void Initialize() => Services.RegisterTick(this);

    public void Tick()
    {
        mediator.Tick();
    }

    public float this[string stat]
    {
        get {
            var query = new Query(stat, stats[stat]);
            mediator.PerformQuery(this, query);
            return query.value;
        }
    }

    public float BaseValue(string stat) => stats[stat];

    public StatsMediator Mediator => mediator;
    public UpdatePriority Priority => ServiceUpdatePriority.Stats;
}



public class StatsMediator
{
    readonly LinkedList<StatModifier> modifiers = new();
    public event EventHandler<Query> Queries;
    
    public void PerformQuery(object sender, Query query) => Queries?.Invoke(sender, query);

    public void AddModifier(StatModifier modifier) 
    {
        modifiers.AddLast(modifier);
        Queries += modifier.Handle;

        modifier.OnDispose += _ => 
        {
            RemoveModifier(modifier);
        };
    }

    public void RemoveModifier(StatModifier modifier)
    {
        modifiers.Remove(modifier);
        Queries -= modifier.Handle;
    }

    public void Tick()
    {
        var node = modifiers.First;
        while (node != null)
        {
            var nextNode = node.Next;

            if (node.Value.MarkedForRemoval)
            {
                node.Value.Dispose();
            }

            node = nextNode;
        }
    }
}

public class Query
{
    public readonly string stat;
    public float value;

    public Query(string stat, float value)
    {
        this.stat  = stat;
        this.value = value;
    }
}


public abstract class StatModifier : IDisposable
{
    public bool MarkedForRemoval { get; private set; }
    readonly    DualCountdown timer;
    public event Action<StatModifier> OnDispose = delegate { };

    protected StatModifier(float duration)
    {
        if (duration <= 0) return;

        timer = new(duration);
        timer.OnTimerStop += () => MarkedForRemoval = true;
        timer.Start();
    }

    protected StatModifier(int duration)
    {
        if (duration <= 0) return;

        timer = new(duration);
        timer.OnTimerStop += () => MarkedForRemoval = true;
        timer.Start();
    }

    public abstract void Handle(object sender, Query query);
    
    public void Dispose()
    {
        OnDispose.Invoke(this);
    }
}


public class BasicStatModifier : StatModifier
{
    readonly string stat;
    readonly Func<float, float> operation;

    public BasicStatModifier(string stat, int frames, Func<float, float> operation) : base(frames)
    {
        this.stat = stat;
        this.operation = operation;
    }

    public BasicStatModifier(string stat, float duration, Func<float, float> operation) : base(duration)
    {
        this.stat = stat;
        this.operation = operation;
    }
    
    public override void Handle(object sender, Query query )
    {
        if (query.stat != stat)
            return;

        query.value = operation(query.value);
    }
}