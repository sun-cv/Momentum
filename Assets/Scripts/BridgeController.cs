using Unity.VisualScripting;
using UnityEngine;




public class BridgeController : Controller
{
    public Bridge Bridge { get; internal set; }
    public void Bind(Bridge bridge) => Bridge??= bridge;
}


public class Bridge
{
    public Actor        Owner           { get; init; }
    public GameObject   View            { get; init; }

    public Bridge(Actor actor, GameObject view)
    {
        Owner       = actor;
        View        = view;

        View.AddComponent<BridgeController>().Bind(this);
    }
}


public class ActorBridge : Bridge
{
    public Rigidbody2D Body             { get; init; }
    public CapsuleCollider2D Collider   { get; init; }
    public Animator Animator            { get; init; }

    public ActorBridge(Actor actor, GameObject view) : base(actor, view)
    {
        Body        = view.GetComponent<Rigidbody2D>();
        Collider    = view.GetComponent<CapsuleCollider2D>();
        Animator    = view.GetComponent<Animator>();
    }
}

public class ProjectileBridge : Bridge
{
    public Rigidbody2D Body             { get; init; }
    public CapsuleCollider2D Collider   { get; init; }
    public SpriteRenderer Sprite        { get; init; }

    public ProjectileBridge(Actor actor, GameObject view) : base(actor, view)
    {
        Body        = view.GetComponent<Rigidbody2D>();
        Collider    = view.GetComponent<CapsuleCollider2D>();
        Sprite      = view.GetComponent<SpriteRenderer>();
    }
}

public class InteractableBridge : Bridge
{
    public CapsuleCollider2D Collider   { get; init; }
    public Animator Animator            { get; init; }

    public InteractableBridge(Actor actor, GameObject view) : base(actor, view)
    {
        Collider    = view.GetComponent<CapsuleCollider2D>();
        Animator    = view.GetComponent<Animator>();
    }
}


