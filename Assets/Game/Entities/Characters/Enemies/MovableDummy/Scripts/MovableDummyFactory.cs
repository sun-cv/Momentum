using UnityEngine;


[Factory(nameof(MovableDummy))]
public class MovableDummyFactory : ICorpseFactory
{

    public Actor Spawn(Vector3 position)
    {
        var definition      = new MovableDummyDefinition();
        var prefab          = Assets.Get(definition.Name);
        var view            = Object.Instantiate(prefab, position, Quaternion.identity);

        MovableDummy dummy  = new();
        dummy.Bridge        = new(dummy, view);

        dummy.Initialize(definition);

        return dummy;
    }
    
    public Actor SpawnCorpse(Actor owner, Vector3 position)
    {
        var definition      = new MovableDummyDefinition();
        var prefab          = Assets.Get(definition.Name);
        var view            = Object.Instantiate(prefab, position, Quaternion.identity);

        MovableDummy dummy  = new();
        dummy.Bridge        = new(dummy, view);

        dummy.Initialize(definition);

        return dummy;
    }
}