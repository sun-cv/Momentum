using UnityEngine;



public class Bridge : Anchor
{
    public Actor Owner              { get; init; }

        // -----------------------------------

    public Rigidbody2D Body         { get; set; }
    public Collider2D FrontZone     { get; set; }
    public Collider2D BackZone      { get; set; }
    public Collider2D Hurtbox       { get; set; }
    public Collider2D Sortbox       { get; set; }

        // -----------------------------------

    public Animator Animator        { get; set; }
    public SpriteRenderer Sprite    { get; set; }

    // ===============================================================================

    public Bridge(Actor actor, GameObject view)
    {
        Owner       = actor;
        View        = view;

        RegisterComponents(view);
    }

    void RegisterComponents(GameObject view)
    {
        foreach (var rule in Rules.Bridge.Parameter.Entries)
        {
            switch(rule.Condition(Owner))
            {
                case true:  rule.Handler(this, view); break;
                case false: break;
            }
        }
    }

}
