using System.Linq;
using UnityEngine;



public class PlayerDriver : ActorService, IServiceTick
{
    readonly InputRouter input;
    readonly WorldPosition world;

    // -----------------------------------

    Vector2 aimVector;
    Vector2 movementVector;

    // ===============================================================================

    public PlayerDriver(Actor owner) : base(owner)
    {
        input = Services.Get<InputRouter>();
        world = Services.Get<WorldPosition>();
    }

    public void Tick()
    {
        UpdateAim();
        UpdateInput();
        UpdateDirection();
        UpdateIntent();
    }

    void UpdateAim()
    {
        aimVector = world.MouseDirectionFrom(owner.Bridge.View.transform.position);
    }

    void UpdateInput()
    {
        MonitorEdgePress();
        MonitorEdgeRelease();
    }

    void MonitorEdgePress()
    {
        var toCreate = input.ActiveButtons.Where(button => button.PressedThisFrame).ToList();

        foreach (var button in toCreate)
        {
            var command     = new CommandAPI()
            {
                Request     = Request.Create,
                Trigger     = IntentMap.Triggers[button.Input],
            };

            owner.Bus.Emit.Local<Request, CommandAPI>(command); 
        }
    }

    void MonitorEdgeRelease()
    {
        var toRemove = input.ActiveButtons.Where(button => button.ReleasedThisFrame).ToList();

        foreach (var button in toRemove)
        {
            var command     = new CommandAPI()
            {
                Request     = Request.Release,
                Trigger     = IntentMap.Triggers[button.Input],
            };

            owner.Bus.Emit.Local<Request, CommandAPI>(command); 
        };
    }

    void UpdateDirection()
    {
        movementVector = input.MovementDirection;
    }

    void UpdateIntent()
    {
        owner.Bus.Emit.Local(new ActorAim(aimVector));
        owner.Bus.Emit.Local(new ActorMovement(movementVector));
    }

    readonly Logger Log = Logging.For(LogSystem.Input);

    public UpdatePriority Priority => ServiceUpdatePriority.ActorDriver; 
}
