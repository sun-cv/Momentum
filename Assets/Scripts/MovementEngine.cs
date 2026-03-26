using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MovementEngine : RegisteredService, IServiceTick
{

    readonly List<IMovementProcessor> processors    = new();
    readonly List<Actor> registeredActors           = new();

    // ===============================================================================

    public MovementEngine()
    {
        Services.Lane.Register(this);
        RegisterProcessors();
    }

    void RegisterProcessors()
    {
        processors.Add(new MovementFrictionProcessor());
        processors.Add(new MovementCombineProcessor());
        processors.Add(new MovementVelocityProcessor());
    }

    // ===============================================================================

    public void Tick()
    {
        RefreshRegistrations();
        ProcessActors();
    }


    void ProcessActors()
    {
        foreach(var actor in registeredActors)
        {
            Process(actor);
        }
    }


    void Process(Actor actor)
    {
        foreach(var processor in processors)
        {
            processor.Process(actor);
        }
    }


    public void RefreshRegistrations()
    {
        if (Actors.GetInterface<IMovableActor>().Count() == registeredActors.Count)
            return;

        registeredActors.Clear();

        foreach (var actor in Actors.GetInterface<IMovableActor>())
        {
            Register(actor.Bridge);
        }
    }

    public void Register(Bridge bridge)
    {
        registeredActors.Add(bridge.Owner);
    }
    

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.MovementEngine);

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.MovementEngine;
}


public interface IMovementProcessor
{
    void Process(Actor actor);
}

public class MovementCombineProcessor : IMovementProcessor
{
    readonly Logger Log = Logging.For(LogSystem.MovementEngine);


    public void Process(Actor actor)
    {
        if (actor is not IMovableActor movable)
            return;

        if (movable.Disabled)
            return;

        Vector2 control = movable.Control;
        Vector2 force   = actor is IPhysicsBody physics ? physics.Force : Vector2.zero;

        if (actor is Hero) Log.Debug("Control", () => $"{control}");
        if (actor is Hero) Log.Debug("Force",   () => $"{force}");
        if (actor is Hero) Log.Debug("Velocity",() => $"{control + force}");
        movable.Velocity = control + force;
    }
}


public class MovementFrictionProcessor : IMovementProcessor
{
    readonly Logger Log = Logging.For(LogSystem.MovementEngine);

        public void Process(Actor actor)
        {
            if (actor is not IMovableActor movable)
                return;

            if (movable.Disabled)
                return;

            if (actor is IPhysicsBody physics)
            {
                physics.Force *= Mathf.Exp(-physics.Friction * Clock.DeltaTime);
                if (physics.Force.magnitude < 0.01f)
                    physics.Force = Vector2.zero;
            }


            movable.Control *= Mathf.Exp(-movable.Friction * Clock.DeltaTime);
            if (movable.Control.magnitude < 0.01f)
                movable.Control = Vector2.zero;
        }
}


public class MovementVelocityProcessor : IMovementProcessor
{
    readonly Logger Log = Logging.For(LogSystem.MovementEngine);

    public void Process(Actor actor)
    {
        var movable = actor as IMovableActor;

        if (movable.Disabled)
            return;
            
        var body     = actor.Bridge.Body;
        var velocity = movable.Velocity;

        if (actor is Hero) Log.Debug("Magnitude",() => $"{velocity.magnitude}");

        if (velocity.magnitude < 0.01f)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }
            
        if (actor is Hero) Log.Debug("Final Velocity",() => $"{velocity}");

        body.linearVelocity = velocity;
    }
}
