using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.Common.Events;
using Game.Realm;

using Event = Game.Common.Event;



namespace Game.Graphics
{

    public class CueLayer
    {
        private readonly List<CueChannel> channels = new() { new ShakeChannel(), new ZoomChannel() };

        private readonly List<CameraCue> cues = new();

        public CueLayer()
        {
            Event.Register<CueLayer, CueCamera>();
            Event.Register<CueLayer, StopCameraCue>();
        }

        public CueResult Result { get; private set; } = new() { Zoom = 1f };

        public void Tick(World world, Vector2 focus)
        {
            Receive();
            Play(world, focus);
            Resolve();
        }

        private void Receive()
        {
            foreach (var message in Event.Read<CueLayer, CueCamera>())
                Start(message);

            foreach (var message in Event.Read<CueLayer, StopCameraCue>())
                Stop(message);
        }

        private void Start(CueCamera message)
        {
            if (Find(message.Cue.Id, message.Source, out var running) || Find(message.Cue.From, message.Source, out running))
            {
                running.Update(message);
                return;
            }

            cues.Add(new CameraCue(message));
        }

        private void Stop(StopCameraCue message)
        {
            foreach (var cue in cues)
            {
                if (cue.Matches(message.Id, message.Source) && cue.Owns(message.Owner))
                    cue.Release();
            }
        }

        private bool Find(string id, Entity? source, out CameraCue match)
        {
            foreach (var cue in cues)
            {
                if (!cue.Matches(id, source))
                    continue;

                match = cue;
                return true;
            }

            match = null;
            return false;
        }

        private void Play(World world, Vector2 focus)
        {
            for (var i = cues.Count - 1; i >= 0; i--)
            {
                var cue = cues[i];

                if (cue.Orphaned(world))
                    cue.Release();

                cue.Advance(focus);

                if (cue.Done)
                    cues.RemoveAt(i);
            }
        }

        private void Resolve()
        {
            var result = new CueResult { Lean = Vector3.zero, Zoom = 1f };

            foreach (var channel in channels)
                channel.Resolve(cues, ref result);

            Result = result;
        }
    }

    public struct CueResult
    {
        public Vector3 Lean { get; set; }
        public float   Zoom { get; set; }
    }

    public abstract class CueChannel
    {
        public abstract void Resolve(IReadOnlyList<CameraCue> cues, ref CueResult result);
    }

    public class ShakeChannel : CueChannel
    {
        private static readonly Vector2 Seed = new(13.7f, 71.3f);

        private float clock;

        public override void Resolve(IReadOnlyList<CameraCue> cues, ref CueResult result)
        {
            var amplitude = 0f;
            var frequency = 0f;

            for (var i = 0; i < cues.Count; i++)
            {
                if (cues[i].Amplitude <= amplitude)
                    continue;

                amplitude = cues[i].Amplitude;
                frequency = cues[i].Frequency;
            }

            clock += frequency * Watch.Tick.UnscaledDelta;

            var x = Mathf.PerlinNoise(Seed.x, clock) * 2f - 1f;
            var y = Mathf.PerlinNoise(Seed.y, clock) * 2f - 1f;

            result.Lean += new Vector3(x, y, 0f) * amplitude;
        }
    }

    public class ZoomChannel : CueChannel
    {
        public override void Resolve(IReadOnlyList<CameraCue> cues, ref CueResult result)
        {
            var closer = 0f;
            var wider  = 0f;

            for (var i = 0; i < cues.Count; i++)
            {
                closer = Mathf.Max(closer, cues[i].Zoom);
                wider  = Mathf.Min(wider,  cues[i].Zoom);
            }

            result.Zoom *= Mathf.Exp(-(closer + wider));
        }
    }

    public class CameraCue
    {
        private Cue     cue;
        private Vector2 origin;
        private Entity? source;
        private Entity? owner;

        private float   time;
        private float   released;
        private bool    releasing;
        private float   fromAmplitude;
        private float   fromFrequency;
        private float   fromZoom;
        private float   amplitude;
        private float   frequency;
        private float   zoom;

        public CameraCue(CueCamera message)
        {
            frequency = message.Cue.Shake.Frequency;

            Update(message);
        }

        public bool  Done           => releasing && time - released >= Seconds(cue.Out);
        public float Frequency      => frequency;
        public float Amplitude      { get; private set; }
        public float Zoom           { get; private set; }

        public bool Matches(string id, Entity? other)
        {
            return !string.IsNullOrEmpty(id) && cue.Id == id && source == other;
        }

        public bool Owns(Entity? other)
        {
            return owner == other;
        }

        public bool Orphaned(World world)
        {
            return owner is Entity entity && !world.Entity.Alive(entity);
        }

        public void Update(CueCamera message)
        {
            cue             = message.Cue;
            origin          = message.Origin;
            source          = message.Source;
            owner           = message.Owner;
            fromAmplitude   = amplitude;
            fromFrequency   = frequency;
            fromZoom        = zoom;
            time            = 0f;
            releasing       = false;
        }

        public void Release()
        {
            if (releasing)
                return;

            releasing       = true;
            released        = time;
            fromAmplitude   = amplitude;
            fromZoom        = zoom;
        }

        public void Advance(Vector2 focus)
        {
            time += Watch.Tick.UnscaledDelta;

            if (!releasing && !cue.Sustained && time >= Seconds(cue.In + cue.Hold))
                Release();

            if (releasing)
            {
                var fall    = Fall();

                amplitude   = fromAmplitude * fall;
                zoom        = fromZoom * fall;
            }
            else
            {
                var rise    = Rise();

                amplitude   = Mathf.Lerp(fromAmplitude, cue.Shake.Amplitude, rise);
                frequency   = Mathf.Lerp(fromFrequency, cue.Shake.Frequency, rise);
                zoom        = Mathf.Lerp(fromZoom,      cue.Zoom,            rise);
            }

            var falloff = Falloff(focus);

            Amplitude   = amplitude * falloff;
            Zoom        = zoom * falloff;
        }

        private float Rise()                    => cue.In  > 0 ? Mathf.Clamp01(time / Seconds(cue.In)) : 1f;
        private float Fall()                    => cue.Out > 0 ? 1f - Mathf.Clamp01((time - released) / Seconds(cue.Out)) : 0f;
        private float Falloff(Vector2 focus)    => cue.Radius > 0f ? 1f - Mathf.Clamp01(Vector2.Distance(focus, origin) / cue.Radius) : 1f;

        private static float Seconds(int ticks) => ticks * Watch.Tick.Delta;
    }
}
