using UnityEngine;



public class AimingSystem : ActorService, IServiceTick
{
    public Vector2 rawAim       = new();
    public Direction aim        = new(Vector2.down);

    public AimingSystem(IntentSystem intent) : base(intent.Owner)
    {
        owner.Bus.Link.Local<ActorAim>(WriteRawAim);
    }

    // ===============================================================================

    public void Tick()
    {
        UpdateAim();
    } 

    // ===============================================================================

    void UpdateAim()
    {
        aim = rawAim.normalized;
    }    

    // ===============================================================================
    //  Events
    // ===============================================================================

    void WriteRawAim(ActorAim message)
    {
        rawAim = message.Vector;
    }

    // ===============================================================================

    public Vector2 RawAim               => rawAim;
    public Direction Aim                => aim;

    public UpdatePriority Priority      => ServiceUpdatePriority.IntentSystem;
}


