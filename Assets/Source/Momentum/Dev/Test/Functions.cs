using Game.Realm;
using Game.Common;
using Game.Content;
using Game.Common.Events;
using System;
using System.Collections.Generic;



namespace Game.Development
{

    public static partial class Test
    {
        public static class Functions
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
    }
}
