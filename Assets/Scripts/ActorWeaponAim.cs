using UnityEngine;



public static class WeaponAimProcessor
{

    public static void Process(WeaponInstance weapon, Actor owner, float deltaTime, out bool facingChanged)
    {
        facingChanged = false;

        var action = weapon.Action;
        var state  = weapon.State;

        state.LiveIntent    = BuildLiveIntent(owner, state.Intent);

        if (action.Aim.LockOnFire && state.Phase == WeaponPhase.Fire)
            return;

        float rawAngle      = ResolveRawAngle(state.LiveIntent, state.CurrentAimAngle);
        float snappedAngle  = ApplyAimMode(rawAngle, action.Aim.Mode);
        float targetAngle   = ApplyHysteresis(snappedAngle, state.TargetAimAngle, action.Aim.HysteresisAngle);

        if (action.Aim.ArcLimitDegs > 0f)
            targetAngle = ApplyArcLimit(targetAngle, state.AnchorAimAngle, action.Aim.ArcLimitDegs);

        state.TargetAimAngle  = targetAngle;
        state.CurrentAimAngle = MoveTowardAngle(state.CurrentAimAngle, state.TargetAimAngle, action.Aim.RotationRateDegs, deltaTime);
    
        Direction resolved = Orientation.DirectionFromAngle(state.CurrentAimAngle);

        facingChanged = resolved.AsIntercardinal != state.LastFacingDirection.AsIntercardinal;

        if (facingChanged)
            state.LastFacingDirection = resolved;
    }

    public static void InitialiseAim(WeaponInstance weapon, Actor owner)
    {
        var state = weapon.State;

        state.LiveIntent      = BuildLiveIntent(owner, state.Intent);

        float initialAngle    = ResolveRawAngle(state.LiveIntent, 0f);
        initialAngle          = ApplyAimMode(initialAngle, weapon.Action.Aim.Mode);

        state.CurrentAimAngle = initialAngle;
        state.TargetAimAngle  = initialAngle;
        state.AnchorAimAngle  = initialAngle;
    }

    static InputIntentSnapshot BuildLiveIntent(Actor owner, InputIntentSnapshot stored)
    {
        Direction liveAim = ((IAimable)owner).Aim;

        return new InputIntentSnapshot
        {
            Aim           = liveAim.HasValue ? liveAim : stored.Facing,
            Facing        = stored.Facing,
            Direction     = stored.Direction,
            LastDirection = stored.LastDirection,
        };
    }

    static float ResolveRawAngle(InputIntentSnapshot intent, float currentAngle)
    {
        if (intent.Aim.HasValue)
            return intent.Aim.Angle;

        return currentAngle;
    }

    static float ApplyAimMode(float angle, AimMode mode)
    {
        return mode switch
        {
            AimMode.Cardinal      => SnapToNearest(angle, 90f),
            AimMode.Intercardinal => SnapToNearest(angle, 45f),
            _                     => angle,
        };
    }

    static float SnapToNearest(float angle, float stepDegrees)
    {
        return Mathf.Round(angle / stepDegrees) * stepDegrees;
    }

    static float ApplyHysteresis(float candidate, float currentTarget, float threshold)
    {
        if (threshold <= 0f)
            return candidate;

        float delta = Mathf.Abs(DeltaAngle(candidate, currentTarget));
        return delta >= threshold ? candidate : currentTarget;
    }

    static float ApplyArcLimit(float targetAngle, float anchorAngle, float arcHalf)
    {
        float delta   = DeltaAngle(anchorAngle, targetAngle);
        float clamped = Mathf.Clamp(delta, -arcHalf, arcHalf);
        return NormalizeAngle(anchorAngle + clamped);
    }

    static float MoveTowardAngle(float current, float target, float maxDegreesPerSec, float deltaTime)
    {
        if (maxDegreesPerSec <= 0f)
            return target;

        float maxDelta = maxDegreesPerSec * deltaTime;
        float delta    = DeltaAngle(current, target);

        if (Mathf.Abs(delta) <= maxDelta)
            return target;

        return NormalizeAngle(current + Mathf.Sign(delta) * maxDelta);
    }

    static float DeltaAngle(float from, float to)
    {
        float delta = NormalizeAngle(to - from);
        if (delta >  180f) delta -= 360f;
        if (delta < -180f) delta += 360f;
        return delta;
    }

    static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0f) angle += 360f;
        return angle;
    }
}

public enum AimMode
{
    Locked,
    Free,
    Cardinal,
    Intercardinal,
}
