using Unity.VisualScripting;
using UnityEngine;



public class Bridge
{
    public Entity               owner;
    public GameObject           instance;

    public Bridge(Entity entity)
    {
        owner       = entity;
        instance    = entity.Instance;
    }
}

public class CharacterBridge : Bridge
{
    public Rigidbody2D          body;
    public CapsuleCollider2D    hitbox;
    public Animator             animator;

    public CharacterBridge(Entity entity) : base(entity)
    {
        body        = instance.GetComponent<Rigidbody2D>();
        hitbox      = instance.GetComponent<CapsuleCollider2D>();
        animator    = instance.GetComponent<Animator>();
    }
}


