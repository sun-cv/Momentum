using UnityEngine;

public static class HeroFactory
{
    public static Hero Create(HeroDefinition definition = null)
    {
        definition ??= new HeroDefinition();

        var prefab      = Registry.Prefabs.Get(definition.Name);
        var instance    = Object.Instantiate(prefab);

        var hero    = new Hero();
        hero.Bridge = new(hero, instance);
        hero.Initialize(definition);
        
        return hero;
    }
    
    public static Hero Create(GameObject view, HeroDefinition definition = null)
    {
        definition ??= new HeroDefinition();

        var hero    = new Hero();
        hero.Bridge = new Bridge(hero, view);

        hero.Initialize(definition);
        return hero;
    }
}