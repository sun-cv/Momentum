using System.Linq;
using UnityEngine;



public class PlayerDriver : ActorService, IServiceTick
{
    readonly InputRouter input;
    readonly WorldPosition worldPosition;

    // -----------------------------------

    Vector2 aimVector;
    Vector2 movementVector;

    // ===============================================================================

    public PlayerDriver(Actor owner) : base(owner)
    {
        input = Services.Get<InputRouter>();
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
        aimVector = worldPosition.MouseDirectionFrom(owner.Bridge.View.transform.position);
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
                Capability      = button.Input,
            };

            var capability  = IntentMap.Triggers[button.Input];
            owner.Bus.Emit.Local(command.Request, command); 
        }
    }

    void MonitorEdgeRelease()
    {
        var toRemove = input.ActiveButtons.Where(button => button.ReleasedThisFrame).ToList();

        foreach (var button in toRemove)
        {
            var command     = new CommandAPI()
            {
                Request     = Request.Destroy,
                Capability  = button.Input,
            };

            owner.Bus.Emit.Local(command.Request, command); 
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

    public UpdatePriority Priority => ServiceUpdatePriority.ActorDriver; 
}
