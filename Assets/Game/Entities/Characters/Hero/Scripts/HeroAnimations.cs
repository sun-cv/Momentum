using Unity.VisualScripting;
using UnityEngine;




public static class HeroAnimation
{
    public static readonly AnimatorRequest Idle         = new(nameof(Idle));
    public static readonly AnimatorRequest SwordStrike  = new(nameof(SwordStrike));
}