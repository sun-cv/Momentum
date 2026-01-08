using UnityEngine;





public readonly struct HeroCameraTarget : IAdvancedCameraTarget
{
    public Hero Hero                        { get; init; }
    public readonly bool IsValid            => Hero != null;
    public readonly Vector3 GetPosition()   => Hero.Character.transform.position;

    public readonly bool IsMoving           => Hero.IsMoving;
    public readonly Vector2 Velocity        => Hero.Velocity; 
    public readonly TimePredicate Idle      => Hero.IsIdle;
}