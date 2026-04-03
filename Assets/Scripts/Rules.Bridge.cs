using System;
using System.Collections.Generic;
using UnityEngine;



public class BridgeParameter
{

    public struct Entry
    {
        public Func<Actor, bool> Condition          { get; set; }
        public Action<Bridge, GameObject> Handler   { get; set; }
    }

    public List<Entry> Entries = new()
    {        
        new()
        {
            Condition   = (actor)           => true,
            Handler     = (bridge, view)    => 
            {
                bridge.Body         = view.GetComponent<Rigidbody2D>();
            }
        },
        new()
        {
            Condition   = (actor)           => true,
            Handler     = (bridge, view)    => 
            {
                bridge.Animator    = view.GetComponent<Animator>();
            }
        },      
        new()
        {
            Condition   = (actor)           => true,
            Handler     = (bridge, view)    => 
            {
                bridge.Sprite       = view.GetComponent<SpriteRenderer>();
            }
        },      
        new()
        {
            Condition   = (actor)           => true,
            Handler     = (bridge, view)    => 
            {
                view.AddComponent<BridgeController>().Bind(bridge);
            }
        },      
        new()
        {
            Condition   = (actor)           => actor is IDamageable,
            Handler     = (bridge, view)    => 
            {
                bridge.Sortbox      = view.GetComponentInChildren<HurtboxController>().Hitbox;
            }
        },      
        new()
        {
            Condition   = (actor)           => actor is IDepthSorted,
            Handler     = (bridge, view)    => 
            {
                bridge.Sortbox      = view.GetComponentInChildren<SortboxController>().Hitbox;
            }
        },
        new()
        {
            Condition   = (actor)           => actor is IDepthColliding,
            Handler     = (bridge, view)    => 
            {
                bridge.FrontZone    = view.GetComponentInChildren<DepthController>().frontZone;
                bridge.BackZone     = view.GetComponentInChildren<DepthController>().backZone;        
            }
        }
    };
}
