using UnityEngine;


namespace Momentum
{

public class AnimatorRequest
{
    public string   name;
    public int      hash;
    public int      layer;
    public float    crossfade;
}


public static class HeroAnimation
{
    public static readonly AnimatorRequest Idle            = new() { name ="Idle",         hash = Animator.StringToHash("Idle"),        layer = 0, crossfade = .1f };
    public static readonly AnimatorRequest Locomotion      = new() { name ="Locomotion",   hash = Animator.StringToHash("Locomotion"),  layer = 0, crossfade = .1f };
    public static readonly AnimatorRequest BasicAttack     = new() { name ="BasicAttack",  hash = Animator.StringToHash("BasicAttack"), layer = 0, crossfade = .1f };
}
}