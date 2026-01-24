




public class DirectionHandler: IServiceTick
{
    Actor           owner;
    IOrientable     actor;
    IntentSystem    intent;
    
    Direction lockedAim;
    Direction lockedFacing;
    Direction lockedDirection;
    
    bool wasAbleToRotate = true;

    public DirectionHandler(Actor actor)
    {
        if (owner is not IOrientable orientable)
        {
            Log.Error(LogSystem.Direction, LogCategory.Activation, () => $"Direction Handler activation requires IOrientable actor (actor {actor.RuntimeID} failed)");
            return;
        }

        this.owner = actor;
        this.actor = orientable;
    }

    public void Tick()
    {
        bool canRotate = actor.CanRotate;
        
        if (wasAbleToRotate && !canRotate)
        {
            lockedAim       = intent.Input.Aim;
            lockedFacing    = intent.Input.Facing;
            lockedDirection = intent.Input.Direction;
        }
        
        wasAbleToRotate = canRotate;
    }

    public Direction LiveAim            => intent.Input.Aim;
    public Direction LiveFacing         => intent.Input.Facing;
    public Direction LiveDirection      => intent.Input.Direction;
    
    public Direction LockedAim          => actor.CanRotate ? LiveAim : lockedAim;
    public Direction LockedFacing       => actor.CanRotate ? LiveFacing : lockedFacing;
    public Direction LockedDirection    => actor.CanRotate ? LiveDirection : lockedDirection;
    
    public UpdatePriority Priority      => ServiceUpdatePriority.DirectionHandler;
}