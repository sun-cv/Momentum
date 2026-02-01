using UnityEngine;

public static class DummyFactory
{
    public static Dummy Create()
    {
        var prefab      = Assets.Get("Dummy");
        var instance    = Object.Instantiate(prefab);

        Dummy dummy     = new();
        dummy.Bridge    = new(dummy, instance);

        dummy.Initialize();
        
        return dummy;
    }
    
    public static Dummy Create(GameObject view)
    {
        Dummy dummy  = new();
        dummy.Bridge = new(dummy, view);

        dummy.Initialize();
        return dummy;
    }
}