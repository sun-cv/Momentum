using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class FacingIntent : ActorService, IServiceTick
{
    readonly IntentSystem intent;

        // -----------------------------------

    readonly Dictionary<Guid, FacingClaim> claims   = new();
    readonly List<FacingAPI> queue                  = new();

        // -----------------------------------

    readonly EffectCache rotationLocks;
    readonly IFacingResolver resolver;

        // -----------------------------------

    readonly Guid defaultClaimant = Guid.NewGuid();

        // -----------------------------------

    Direction facing = new(Vector2.down);

   // ===============================================================================

    public FacingIntent(IntentSystem intent) : base(intent.Owner)
    {
        this.intent = intent;

        resolver = FacingProfiles.Create(definition.Behaviour.Facing);

        rotationLocks = new(owner.Bus, effect => effect is IDisableRotate);

        owner.Bus.Link.Local<Message<Request, FacingAPI>>(HandleFacingRequest);

        SeedDefaultClaim();
    }

   // ===============================================================================

    public void Tick()
    {
        ProcessQueue();
        ResolveFacing();
    }

   // ===============================================================================

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
        SeedDefaultClaim();
    }

    void SeedDefaultClaim() 
    {
        var baseline = new FacingClaim(defaultClaimant);
 
        baseline.Apply(new FacingAPI
        {
            Claimant    = defaultClaimant,
            Priority    = global::Priority.Default,
            Mode        = DirectionMode.Live,
            Source      = DirectionSource.Direction,
            Constraint  = DirectionConstraint.Free,
        });

        claims[defaultClaimant] = baseline;
    }

    // ===============================================================================
    //  Helpers
    // ===============================================================================

    Direction ResolveClaim(FacingClaim claim)
    {
        if (claim.Constraint == DirectionConstraint.Locked && claim.HasLockedFacing)
            return claim.LockedFacing;

        var resolved = ResolveSource(claim);

        if (ShouldQuantize(claim) && resolved.HasValue)
            resolved = resolver.Resolve(new FacingContext
            {
                Current     = facing,
                Target      = resolved,
                Settle      = intent.Direction.DiagonalTravel,
            });

        if (claim.Constraint == DirectionConstraint.Locked && resolved.HasValue)
        {
            claim.LockedFacing = resolved;
            claim.HasLockedFacing = true;
        }

        return resolved;
    }

    bool ShouldQuantize(FacingClaim claim)
    {
        return claim.Mode == DirectionMode.Live && claim.Source == DirectionSource.Direction;
    }

    Direction ResolveSource(FacingClaim claim)
    {
        return claim.Mode switch
        {
            DirectionMode.Live          => ResolveLiveSource(claim.Source, claim),
            DirectionMode.Snapshot      => ResolveSnapshotSource(claim.Source, claim),
            _                           => default,
        };
    }

    Direction ResolveLiveSource(DirectionSource source, FacingClaim claim)
    {
        return source switch
        {
            DirectionSource.Aim         => intent.Aiming.Aim,
            DirectionSource.Direction   => intent.Direction.Direction,
            DirectionSource.Explicit    => claim.Explicit,
            _                           => default,
        };
    }

    Direction ResolveSnapshotSource(DirectionSource source, FacingClaim claim)
    {
        return source switch
        {
            DirectionSource.Aim         => claim.Snapshot.Aim,
            DirectionSource.Direction   => claim.Snapshot.Direction,
            DirectionSource.Explicit    => claim.Explicit,
            _                           => default,
        };
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleFacingRequest(Message<Request, FacingAPI> message)
    {
        queue.Add(message.Payload);
    }

    // ===============================================================================

    public Direction Facing         => facing;
    public UpdatePriority Priority  => ServiceUpdatePriority.FacingHandler;
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


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                              Facing Resolvers
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public interface IFacingResolver
{
    Direction Resolve(in FacingContext context);
}


public readonly struct FacingContext
{
    public Direction Current        { get; init; }    
    public Direction Target         { get; init; }
    public TimePredicate Settle     { get; init; }
}


public class HorizontalSnapResolver : IFacingResolver
{
    public Direction Resolve(in FacingContext context)
    {
        if (Mathf.Approximately(context.Target.X, 0f))
            return context.Current;

        return new Vector2(Mathf.Sign(context.Target.X), 0);
    }
}


public class CardinalSnapResolver : IFacingResolver
{
    public Direction Resolve(in FacingContext context)
    {
        return context.Target.IsZero ? context.Current : context.Target.Cardinal;
    }
}


public class IntercardinalResolver : IFacingResolver
{
    public Direction Resolve(in FacingContext context)
    {
        return context.Target.IsZero ? context.Current : context.Target.Intercardinal;
    }
}


public class DelayedTurnResolver : IFacingResolver
{
    public Direction Resolve(in FacingContext context)
    {
        var target = context.Target;

        if (target.IsZero)
            return context.Current;

        if (Mathf.Abs(target.Y) < 0.01f)
            return new Vector2(target.X > 0 ? 1 : -1, 0);

        if (Mathf.Abs(target.X) < 0.01f)
            return new Vector2(0, target.Y > 0 ? 1 : -1);

        if (context.Settle.Duration >= Orientation.GetTurnDelay(context.Current, target))
            return new Vector2(target.X > 0 ? 1 : -1, 0);

        return context.Current;
    }
}


public enum FacingProfile
{
    HorizontalSnap,
    CardinalSnap,
    DelayedTurn,
    Intercardinal,
}


public static class FacingProfiles
{
    static readonly Dictionary<FacingProfile, Func<IFacingResolver>> map = new()
    {
        { FacingProfile.HorizontalSnap, () => new HorizontalSnapResolver()  },
        { FacingProfile.CardinalSnap,   () => new CardinalSnapResolver()    },
        { FacingProfile.DelayedTurn,    () => new DelayedTurnResolver()     },
        { FacingProfile.Intercardinal,  () => new IntercardinalResolver()   },
    };

    public static IFacingResolver Create(FacingProfile profile) => map[profile]();
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

    public FacingAPI(AbilityFacing definition)
    {
        Mode        = definition.DirectionMode;
        Source      = definition.DirectionSource;
        Constraint  = definition.DirectionConstraint;
    }

    public FacingAPI()
    {

    }
}


