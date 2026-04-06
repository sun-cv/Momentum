using UnityEngine;



[Factory(nameof(Hero))]
public class HeroFactory : IRespawnFactory, ICorpseFactory
{
    public Actor Spawn(Vector3 position)
    {
        var definition  = new HeroDefinition();

        var prefab      = Assets.Get(definition.Name);
        var view        = Object.Instantiate(prefab, position, Quaternion.identity);

        var hero        = new Hero();

        hero.Bridge     = new(hero, view);
        hero.Initialize(definition);
        
        return hero;
    }

    public Actor SpawnCorpse(Vector3 position)
    {
        var definition  = new HeroDefinition();
        var prefab      = Assets.Get(definition.Lifecycle.Spawn.Corpse);
        var view        = Object.Instantiate(prefab, position, Quaternion.identity);

        var hero        = new Hero();

        hero.Bridge     = new(hero, view);
        hero.Initialize(definition);
        
        return hero;
    }

}
