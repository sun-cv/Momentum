using UnityEngine;



namespace Game.Common
{
    public static partial class Config
    {
        public static class Gizmo
        {
            public readonly struct Style
            {
                public Color Line   { get; }
                public Color Fill   { get; }

                public Style(Color line, Color fill)
                {
                    Line = line;
                    Fill = fill;
                }
            }

            public static readonly bool  Hitboxes   = true;
            public static readonly Style Hitbox     = new(new( 1f, .2f, .2f, .5f), new( 1f, .2f, .2f, .1f));
            public static readonly Style Hurtbox    = new(new(.2f, .2f,  1f, .5f), new(.2f, .2f,  1f, .1f));
            public static readonly Style Bodybox    = new(new(.2f,  1f, .2f, .1f), new(.2f,  1f, .2f, .00f));
        }
    }
}
