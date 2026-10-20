using Game.Realm;
using Game.Common;
using Game.Content;
using Game.Common.Events;
using System;
using System.Collections.Generic;



namespace Game.Diagnostic
{

    public static partial class Test
    {
        public static class Timeline
        {
            public static void Loop(int modulus, List<(int, Action)> ticks)
            {
                foreach(var (tick, callback) in ticks)
                {
                    if (Watch.Tick.Game % modulus == tick)
                        callback();
                }
            }
        }

        public static class Camera 
        {
            public static void LockOn(World World, Assets Asset, Entity entity)
            {
                if (Watch.Tick.Game % 600 != 300)
                    return;

                var target  = World.Entity.Create(Asset.Get("Dummy"), new() { Position = new() { x = -4, y = -2, z = 0 }});
                var focus   = World.Entity.Create(new Definition { Id = "TestLockOn", Focus = new Focus(), Duration = new Duration { Length = 180 }, Highlight = new Highlight { Takeover = 0.8f, Center = 1f, In = 60, Out = 90, Stickiness = 45, Limit = 7f }}, entity);

                World.Entity.Component.Add(focus,  new CameraFocus { Entity = target });
                World.Entity.Component.Add(target, new Bound { Entity = focus });
            }

            public static void Shake()
            {
                List<(int, Action)> entries = new() 
                {
                    ( 30,  () => Event.Send(new CueCamera { Cue = new Cue { Id = "TestShake", In = 30, Out = 30, Sustained = true, Shake = new Shake { Amplitude = 0.03f, Frequency = 6f } } })),
                    ( 90,  () => Event.Send(new CueCamera { Cue = new Cue { Id = "TestShake", In = 90, Out = 30, Sustained = true, Shake = new Shake { Amplitude = 0.02f, Frequency = 30f } } })),
                    ( 240, () => Event.Send(new StopCameraCue { Id = "TestShake" }))
                };

                Timeline.Loop(600, entries);
            }
        }
    }
}
