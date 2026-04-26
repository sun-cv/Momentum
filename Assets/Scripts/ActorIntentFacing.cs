using System;
using System.Collections.Generic;
using UnityEngine;



public class FacingIntent : ActorService, IServiceTick
{

    readonly DirectionIntent source;

        // -----------------------------------

    readonly List<FacingAPI> queue                                              = new();

        // -----------------------------------

    readonly List<Priority> tiers                                               = new();
    readonly Dictionary<Priority, (Guid Claimant, Direction Direction)> claims  = new();

        // -----------------------------------

    readonly EffectCache locks;

        // -----------------------------------

    Direction facing    = new(Vector2.down);

   // ===============================================================================

    public FacingIntent(IntentSystem intent) : base (intent.Owner)
    {
        source  = intent.Direction;
        locks   = new(owner.Bus,(effect) => effect is IDisableRotate);

        CreateTiers();
    }

    // ===============================================================================

    public void Tick()
    {
        ProcessClaims();
        ResolveFacing();
    }

    void ProcessClaims()
    {
        ProcessQueue();
    }

    void ProcessQueue()
    {
        foreach (var request in queue)
        {
            Process(request); 
        }
    }

    void Process(FacingAPI message)
    {
        switch(message.Request)
        {
            case Request.Claim:     ClaimFacing(message);   break;
            case Request.Release:   ReleaseFacing(message); break;
        }
    }

    void ResolveFacing()
    {
        if (ResolveClaimedFacing())
            return;

        if (ResolveLockedFacing())
            return;

        if (ResolveBaseFacing())
            return;

        if (ResolveDefaultFacing())
            return;
    }
    
    bool ResolveClaimedFacing()
    {
        if (claims.Count == 0)
            return false;

        foreach (var tier in tiers)
        {
            if (claims.TryGetValue(tier, out var claim))
            {
                facing = claim.Direction;
                return true;
            }
        }
        return false;
    }

    bool ResolveLockedFacing()
    {
        return locks.Count > 0;
    }

    bool ResolveBaseFacing()
    {
        if (source.Direction.IsZero)
            return false;

        if (!source.Direction.IsCardinal)
            return false;

        bool facingHorizontal = Mathf.Abs(facing.X) > Mathf.Abs(facing.Y);

        if (facingHorizontal)
        {
            facing = new Vector2(source.Direction.X > 0 ? 1 : -1, 0);
            return true;
        }

        if (!facingHorizontal)
        {
            if (source.DiagonalTravel.Duration >= Orientation.GetTurnDelay(facing, source.Direction))
                facing = new Vector2(source.Direction.X > 0 ? 1 : -1, 0);
        }

        return true;
    }

    bool ResolveDefaultFacing()
    {
        if (source.Direction.IsZero)
            return false;

        facing = source.Direction.Cardinal;
        return true;
    }
    
    // ===============================================================================
    //  Helpers
    // ===============================================================================


    void ClaimFacing(FacingAPI request)
    {
        claims[request.Priority] = (request.Claimant, request.Facing);
    }

    void ReleaseFacing(FacingAPI request)
    {
        if (!claims.TryGetValue(request.Priority, out var claim))
            return;

        if (request.Claimant != claim.Claimant)
            return;

        claims.Remove(request.Priority);
    }

    void CreateTiers()
    {
        foreach (Priority tier in Enum.GetValues(typeof(Priority)))
            tiers.Add(tier);

        tiers.Reverse();
    }
    bool IsMovingDiagonal()
    {
        return source.Direction.IsDiagonal;
    }

    // ===============================================================================

    public Direction Facing         => facing;
    public UpdatePriority Priority  => ServiceUpdatePriority.FacingHandler;

}



public class FacingAPI : API
{
    public Guid Claimant                    { get; init; }
    public Direction Facing                 { get; init; }
    public Priority Priority                { get; init; }

    public FacingAPI(Vector2 facing) => Facing = new Direction(facing);
}



