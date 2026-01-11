




public static class HeroFactory
{
    public static Hero Create(HeroDefinition definition = null)
    {
        definition    ??= new HeroDefinition();

        var prefab      = Registry.Prefabs.Get("HeroController");
        var instance    = UnityEngine.Object.Instantiate(prefab);

        var hero        = new Hero();
        hero.Bridge     = new ActorBridge(hero, instance);

        hero.Initialize(definition);
        
        return hero;
    }
}
