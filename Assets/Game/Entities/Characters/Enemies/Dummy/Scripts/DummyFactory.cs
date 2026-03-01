using UnityEngine;


[Factory(nameof(Dummy))]
public class DummyFactory : ICorpseFactory
{

    public Actor Spawn(Vector3 position)
    {
        var definition  = new DummyDefinition();
        var prefab      = Assets.Get(definition.Name);
        var view        = Object.Instantiate(prefab, position, Quaternion.identity);

        Dummy dummy     = new();
        dummy.Bridge    = new(dummy, view);

        dummy.Initialize(definition);

        return dummy;
    }
    
    public Actor SpawnCorpse(Actor owner, Vector3 position)
    {
        var definition  = new DummyDefinition();
        var prefab      = Assets.Get(definition.Lifecycle.Corpse.Name);
        var view        = Object.Instantiate(prefab, position, Quaternion.identity);

        Dummy dummy     = new();
        dummy.Bridge    = new(dummy, view);

        dummy.Initialize(definition);

        return dummy;
    }
}