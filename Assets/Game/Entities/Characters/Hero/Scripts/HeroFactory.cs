using UnityEngine;





public static class HeroFactory
{
    public static Hero Create(HeroDefinition definition = null)
    {
        definition ??= new HeroDefinition();

        var prefab      = Assets.Get(definition.Name);
        var view        = Object.Instantiate(prefab);

        var hero    = new Hero();
        hero.Bridge = new(hero, view);
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