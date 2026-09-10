


namespace Game.Common
{

    public interface IDefinition 
    {
        public string Id        { get; init; }     
    }

    public static class Definition
    {
        public class Actor : IDefinition
        {
            public string Id        { get; init; }
            public Health? Health   { get; init; }
        }

        public class Prop : IDefinition
        {
            public string Id        { get; init; }    
        }

        public class Spawner : IDefinition
        {
            public string Id        { get; init; }    
        }

        public class Item : IDefinition
        {
            public string Id        { get; init; }    
        }
    }
}



