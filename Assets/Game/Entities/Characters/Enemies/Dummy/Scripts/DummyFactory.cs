using UnityEngine;


[Factory(nameof(Dummy))]
public class DummyFactory : ICorpseFactory
{

    public Actor Spawn(Vector3 position)
    {
        var definition              = new DummyDefinition();
        var prefab                  = Assets.Get(definition.Name);
        var view                    = Object.Instantiate(prefab, position, Quaternion.identity);

        Dummy dummy                 = new();
        dummy.Bridge                = new(dummy, view);

        dummy.Initialize(definition);

        return dummy;
    }
    
    public Actor SpawnCorpse(Vector3 position)
    {
        var definition              = new DummyCorpseDefinition();
        var prefab                  = Assets.Get(definition.Name);
        var view                    = Object.Instantiate(prefab, position, Quaternion.identity);

        DummyCorpse corpse          = new();
        corpse.Bridge               = new(corpse, view);

        corpse.Initialize(definition);

        return corpse;
    }
}
