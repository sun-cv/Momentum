using UnityEngine;



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Controller
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class BridgeController : Controller
{
    public Bridge Bridge                { get; internal set; }
    
    public void Bind(Bridge bridge)
    {
        if (Bridge != null)
            return;

        Bridge = bridge;

        Actors.Register(Bridge.Owner);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        Bridge.Owner.Emit.Local(Request.Create, new CollisionEvent(CollisionPhase.Enter, collision));
    }
    
    public void OnCollisionStay2D (Collision2D collision)
    {
        Bridge.Owner.Emit.Local(Request.Create, new CollisionEvent(CollisionPhase.Stay,  collision));
    }

    public void OnCollisionExit2D (Collision2D collision)
    {
        Bridge.Owner.Emit.Local(Request.Create, new CollisionEvent(CollisionPhase.Exit,  collision));
    }

    public void OnDestroy()
    {
        if (Bridge == null) 
            return;

        Actors.Deregister(Bridge.Owner);

        Bridge = null;
    }
}



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Factories
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public static class BridgeFactory
{
    public static Bridge Create(Actor actor, string prefabName)
    {
        return Instantiate(actor, prefabName);
    }

    public static Bridge Create(IDefined actor)
    {
        return Instantiate((Actor)actor, actor.Definition.Name);
    }

    static Bridge Instantiate(Actor actor, string prefabName)
    {
        var prefab      = Assets.Get(prefabName);
        var instance    = UnityEngine.Object.Instantiate(prefab);

        return new Bridge(actor, instance);
    }
}



