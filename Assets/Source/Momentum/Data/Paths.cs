using System;
using System.Collections.Generic;
using Game.Common;



namespace Game.Content
{

    internal static class DataPath
    {
        internal static readonly Dictionary<Type, string> Locations = new()
        {
            { typeof(Definition.Actor),    "World/Entity/Actor" }
        };
    }
}
