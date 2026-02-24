using UnityEngine;



public class Anchor
{
    public GameObject View          { get; init; }
}



public class Bridge : Anchor
{
    public Actor Owner              { get; init; }

        // -----------------------------------

    public Rigidbody2D Body         { get; init; }
    public Collider2D FrontZone     { get; init; }
    public Collider2D BackZone      { get; init; }
    public Collider2D Hurtbox       { get; init; }
    public Collider2D Sortbox       { get; init; }

        // -----------------------------------

    public Animator Animator        { get; init; }
    public SpriteRenderer Sprite    { get; init; }

    // ===============================================================================

    public Bridge(Actor actor, GameObject view)
    {
        Owner       = actor;
        View        = view;
        
        Body        = view.GetComponent<Rigidbody2D>();
        FrontZone   = view.GetComponentInChildren<DepthController>().frontZone;
        BackZone    = view.GetComponentInChildren<DepthController>().backZone;        
        Hurtbox     = view.GetComponentInChildren<HurtboxController>().Hitbox;
        Sortbox     = view.GetComponentInChildren<SortboxController>().Hitbox;
        Animator    = view.GetComponent<Animator>();
        Sprite      = view.GetComponent<SpriteRenderer>();
        
        view.AddComponent<BridgeController>().Bind(this);
    }
}
