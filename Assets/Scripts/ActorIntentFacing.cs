using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class FacingIntent : ActorService, IServiceTick
{
    readonly IntentSystem intent;

    readonly Dictionary<Guid, FacingClaim> claims = new();
    readonly List<FacingAPI> queue = new();

    readonly EffectCache rotationLocks;

    Direction facing = new(Vector2.down);

    public FacingIntent(IntentSystem intent) : base(intent.Owner)
    {
        this.intent = intent;

        rotationLocks = new(owner.Bus, effect => effect is IDisableRotate);

        owner.Bus.Link.Local<Message<Request, FacingAPI>>(HandleFacingRequest);
    }

    public void Tick()
    {
        ProcessQueue();
        ResolveFacing();
    }

    void ProcessQueue()
    {
        foreach (var request in queue)
        {
            switch (request.Request)
            {
                case Request.Claim:
                    Claim(request);
                    break;

                case Request.Release:
                    Release(request);
                    break;

                case Request.Clear:
                    Clear(request);
                    break;
            }
        }

        queue.Clear();
    }

    void ResolveFacing()
    {
        var claim = WinningClaim();

        if (claim == null)
            return;

        var desired = ResolveClaim(claim);

        if (!desired.HasValue)
            return;

        if (RotationLocked() && !claim.IgnoreRotationLock)
            return;

        facing = desired;
    }

    FacingClaim WinningClaim()
    {
        return claims.Values
            .OrderByDescending(claim => claim.Priority)
            .ThenByDescending(claim => claim.FrameClaimed)
            .FirstOrDefault();
    }

    Direction ResolveClaim(FacingClaim claim)
    {
        if (claim.Constraint == DirectionConstraint.Locked && claim.HasLockedFacing)
            return claim.LockedFacing;

        var resolved = ResolveSource(claim);

        if (claim.Constraint == DirectionConstraint.Locked && resolved.HasValue)
        {
            claim.LockedFacing = resolved;
            claim.HasLockedFacing = true;
        }

        return resolved;
    }

    Direction ResolveSource(FacingClaim claim)
    {
        return claim.Mode switch
        {
            DirectionMode.Live     => ResolveLiveSource(claim.Source, claim),
            DirectionMode.Snapshot => ResolveSnapshotSource(claim.Source, claim),
            _                      => default,
        };
    }

    Direction ResolveLiveSource(DirectionSource source, FacingClaim claim)
    {
        return source switch
        {
            DirectionSource.Aim       => intent.Aiming.Aim,
            DirectionSource.Direction => intent.Direction.Direction,
            DirectionSource.Explicit  => claim.Explicit,
            _                         => default,
        };
    }

    Direction ResolveSnapshotSource(DirectionSource source, FacingClaim claim)
    {
        return source switch
        {
            DirectionSource.Aim       => claim.Snapshot.Aim,
            DirectionSource.Direction => claim.Snapshot.Direction,
            DirectionSource.Explicit  => claim.Explicit,
            _                         => default,
        };
    }

    bool RotationLocked()
    {
        return rotationLocks.Count > 0;
    }

    void Claim(FacingAPI request)
    {
        if (request.Claimant == Guid.Empty)
            return;

        if (!claims.TryGetValue(request.Claimant, out var claim))
        {
            claim = new FacingClaim(request.Claimant);
            claims.Add(request.Claimant, claim);
        }

        claim.Apply(request);
    }

    void Release(FacingAPI request)
    {
        if (request.Claimant == Guid.Empty)
            return;

        claims.Remove(request.Claimant);
    }

    void Clear(FacingAPI request)
    {
        claims.Clear();
    }

    void HandleFacingRequest(Message<Request, FacingAPI> message)
    {
        queue.Add(message.Payload);
    }

    public Direction Facing => facing;
    public UpdatePriority Priority => ServiceUpdatePriority.FacingHandler;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class FacingClaim
{
    public Guid Claimant                    { get; }

    public Priority Priority                { get; private set; }
    public DirectionMode Mode               { get; private set; }
    public DirectionSource Source           { get; private set; }
    public DirectionConstraint Constraint   { get; private set; }

    public IntentSnapshot Snapshot          { get; private set; }
    public Direction Explicit               { get; private set; }

    public bool IgnoreRotationLock          { get; private set; }

    public bool HasLockedFacing             { get; set; }
    public Direction LockedFacing           { get; set; }

    public int FrameClaimed                 { get; private set; }

    public FacingClaim(Guid claimant)
    {
        Claimant = claimant;
    }

    public void Apply(FacingAPI request)
    {
        Priority            = request.Priority;
        Mode                = request.Mode;
        Source              = request.Source;
        Constraint          = request.Constraint;
        Snapshot            = request.Snapshot;
        Explicit            = request.Explicit;
        IgnoreRotationLock  = request.IgnoreRotationLock;

        HasLockedFacing     = false;
        LockedFacing        = default;

        FrameClaimed        = Clock.FrameCount;
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class FacingAPI : API
{
    public Guid Claimant                    { get; init; }
    public Priority Priority                { get; init; } = Priority.Default;

    public DirectionMode Mode               { get; init; } = DirectionMode.Live;
    public DirectionSource Source           { get; init; } = DirectionSource.Direction;
    public DirectionConstraint Constraint   { get; init; } = DirectionConstraint.Free;

    public IntentSnapshot Snapshot          { get; init; }
    public Direction Explicit               { get; init; }

    public bool IgnoreRotationLock          { get; init; }

    public FacingAPI() {}
}



