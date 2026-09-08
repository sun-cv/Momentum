using System;
using System.Collections.Generic;
using Game.Common;



namespace Game.Data
{

    internal static class DefinitionRegistry
    {
        internal static readonly Dictionary<Type, string> Locations = new()
        {
            { typeof(ActorDefinition),    "World/Entity/Actor/Hero" }
        };
    }
}
