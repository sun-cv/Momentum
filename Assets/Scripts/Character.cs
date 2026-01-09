using Unity.VisualScripting;
using UnityEngine;




public class Character
{
    public Entity               owner;
    public GameObject           instance;
    public Rigidbody2D          body    ;
    public CapsuleCollider2D    hitbox  ;
    public Animator             animator;

    public Character(Entity entity)
    {
        owner       = entity;
        instance    = entity.Instance;

        body        = instance.GetComponent<Rigidbody2D>();
        hitbox      = instance.GetComponent<CapsuleCollider2D>();
        animator    = instance.GetComponent<Animator>();
    }
}
