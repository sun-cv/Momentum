using UnityEngine;



namespace Game.Common
{
    public static partial class Config
    {
        public static class Gizmo
        {
            public static readonly bool  Hitboxes   = true;
            public static readonly float Fill       = .25f;
            public static readonly Color Hitbox     = new(1f, .2f, .2f, 1f);
        }
    }
}
