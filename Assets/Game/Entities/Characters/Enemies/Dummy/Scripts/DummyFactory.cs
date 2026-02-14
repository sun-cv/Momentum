using UnityEngine;

public static class DummyFactory
{
    public static Dummy Create()
    {
        var definition  = new DummyDefinition();
        var prefab      = Assets.Get(definition.Name);
        var view        = Object.Instantiate(prefab);

        Dummy dummy     = new();
        dummy.Bridge    = new(dummy, view);

        dummy.Initialize(definition);
        
        return dummy;
    }
    
    public static Dummy Create(GameObject view)
    {
        Dummy dummy  = new();
        dummy.Bridge = new(dummy, view);

        dummy.Initialize(new DummyDefinition());
        return dummy;
    }
}