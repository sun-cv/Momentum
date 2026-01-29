using System.Collections.Generic;





public static class Entities
{
    public static void Register  (Bridge bridge) => Registry.Entities.Register(bridge);
    public static void Deregister(Bridge bridge) => Registry.Entities.Deregister(bridge);
    
    public static IEnumerable<Bridge> GetAll()   => Registry.Entities.GetAll();
    public static IEnumerable<Bridge> GetByInterface<T>() where T : class => Registry.Entities.GetByInterface<T>();
}
