


namespace Game.Common
{

    public interface IDefinition 
    {
        public string Id        { get; init; }     
    }

    public class ActorDefinition : IDefinition
    {
        public string Id        { get; init; }
        public string Name      { get; init; }
    }

    public class PropDefinition : IDefinition
    {
        public string Id        { get; init; }    
    }

    public class SpawnerDefinition : IDefinition
    {
        public string Id        { get; init; }    
    }

    public class ItemDefinition : IDefinition
    {
        public string Id        { get; init; }    
    }
}
