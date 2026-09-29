using System.Collections.Generic;
using UnityEngine;



namespace Game.Common
{
    public sealed class Clip
    {
        public string State                                 { get; }
        public Vector2 Direction                            { get; }
        public IReadOnlyList<Sprite> Frames                 { get; }
        public IReadOnlyList<int> Ticks                     { get; }
        public int Length                                   { get; }
        public bool Loop                                    { get; }

        public Clip(string state, Vector2 direction, IReadOnlyList<Sprite> frames, IReadOnlyList<int> ticks, bool loop)
        {
            State       = state;
            Direction   = direction;
            Frames      = frames;
            Ticks       = ticks;
            Loop        = loop;

            foreach (var tick in ticks)
            {
                Length += tick;
            }
        }
    }

    public sealed class Sheet
    {
        public string Id                                                            { get; }
        public IReadOnlyDictionary<(string State, Vector2 Direction), Clip> Clips   { get; }

        public Sheet(string id, IReadOnlyDictionary<(string State, Vector2 Direction), Clip> clips)
        {
            Id      = id;
            Clips   = clips;
        }
    }
}

