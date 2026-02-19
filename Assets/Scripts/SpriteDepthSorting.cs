using System.Collections.Generic;
using System.Linq;
using UnityEngine;




[Service]
public class SpriteLayeringSystem : Service, IServiceStep, IBind
{
    readonly List<TrackedSprite> trackedSprites = new();
    
    // ===============================================================================

    public void Register(Bridge bridge)
    {
        var tracked = new TrackedSprite 
        { 
            transform   = bridge.Body.transform, 
            sprite      = bridge.Sprite,
            backZone    = bridge.BackZone,
            sortbox     = bridge.Sortbox
        };
        
        trackedSprites.Add(tracked);
    }

    public void Step()
    {
        RefreshRegistrations();
        SortLayers();
    }
    
    public void RefreshRegistrations()
    {
        if (Actors.GetInterface<IDepthSorted>().Count() == trackedSprites.Count)
            return;

        trackedSprites.Clear();
        
        foreach (var bridge in Actors.GetInterface<IDepthSorted>())
        {
            Register(bridge);
        }
    }

    public void SortLayers()
    {
        for (int i = 0; i < trackedSprites.Count; i++)
        {
            for (int j = i + 1; j < trackedSprites.Count; j++)
            {
                var sprite1     = trackedSprites[i];
                var sprite2     = trackedSprites[j];

                var original    = sprite1;
                var expanded    = sprite1;

                expanded.sortbox.bounds.Expand(Config.Rendering.SPRITE_OVERLAP_LOOKAHEAD);

                bool overlapping = original.sortbox.bounds.Intersects(sprite2.sortbox.bounds);
                bool approaching = expanded.sortbox.bounds.Intersects(sprite2.sortbox.bounds);

                if (overlapping || approaching)
                {
                    Log.Trace($"Overlap detected! overlapping={overlapping}, approaching={approaching}");
                }

                if (overlapping || approaching)
                {
                    if (!sprite1.orderOverrides.ContainsKey(sprite2))
                    {
                        float footY1 = sprite1.FootY;
                        float footY2 = sprite2.FootY;

                        if (footY1 < footY2)
                        {
                            sprite1.orderOverrides[sprite2] =  1;
                            sprite2.orderOverrides[sprite1] = -1;
                        }
                        else
                        {
                            sprite1.orderOverrides[sprite2] = -1;
                            sprite2.orderOverrides[sprite1] =  1;
                        }
                    }
                }
                else
                {
                    if (sprite1.orderOverrides.ContainsKey(sprite2))
                    {
                        Log.Trace($"CLEARING override");
                    }

                    sprite1.orderOverrides.Remove(sprite2);
                    sprite2.orderOverrides.Remove(sprite1);
                }
            }
        }

        foreach (var tracked in trackedSprites)
        {
            int finalOrder;

            if (tracked.orderOverrides.Count > 0)
            {
                finalOrder = 0;
                foreach (var offset in tracked.orderOverrides.Values)
                {
                    finalOrder += offset;
                }
            }
            else
            {
                finalOrder = Mathf.RoundToInt(-tracked.FootY * 100);
            }

            tracked.sprite.sortingOrder = finalOrder;
        }
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.SpriteLayering);

    public void Bind()
    {
        RefreshRegistrations();
    }

    public override void Dispose()
    {
        // NO OP;
    }

    public UpdatePriority Priority => new(UpdatePhase.Render, 100);
}



[Service]
public class DepthCollisionSystem : RegisteredService, IServiceStep, IBind
{
    
    List<EntityCollision> trackedSprites = new();

        // ===============================================================================

    public void Register(Bridge bridge)
    {
        trackedSprites.Add(new EntityCollision
        {
            actor       = bridge.Owner,
            frontZone   = bridge.FrontZone,
            backZone    = bridge.BackZone,
            transform   = bridge.Body.transform
        });
    }
    
    public void Step()
    {
        RefreshRegistrations();
        SortDepth();
    }
    
    public void RefreshRegistrations()
    {
        if (Actors.GetInterface<IDepthColliding>().Count() == trackedSprites.Count)
            return;

        trackedSprites.Clear();
        
        foreach (var bridge in Actors.GetInterface<IDepthColliding>())
        {
            Register(bridge);
        }
    }

    void SortDepth()
    {
        for (int i = 0; i < trackedSprites.Count; i++)
        {
            for (int j = i + 1; j < trackedSprites.Count; j++)
            {
                var entity1     = trackedSprites[i];
                var entity2     = trackedSprites[j];
                
                float distance  = Vector2.Distance(entity1.transform.position, entity2.transform.position);
                
                if (distance > 5f) 
                {   
                    continue;
                }

                float y1 = entity1.transform.position.y;
                float y2 = entity2.transform.position.y;
                
                if (y1 < y2)
                {
                    Physics2D.IgnoreCollision(entity1.backZone, entity2.frontZone, false);
                    Physics2D.IgnoreCollision(entity1.frontZone, entity2.frontZone, true);
                    Physics2D.IgnoreCollision(entity1.frontZone, entity2.backZone, true);
                    Physics2D.IgnoreCollision(entity1.backZone, entity2.backZone, true);
                }
                else
                {
                    Physics2D.IgnoreCollision(entity2.backZone, entity1.frontZone, false);
                    Physics2D.IgnoreCollision(entity1.frontZone, entity2.frontZone, true);
                    Physics2D.IgnoreCollision(entity1.backZone, entity2.frontZone, true);
                    Physics2D.IgnoreCollision(entity1.backZone, entity2.backZone, true);
                }
            }
        }
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.DepthSorting);

    public void Bind()
    {
        RefreshRegistrations();
    }

    public override void Dispose()
    {
        // NO OP;
    }

    public UpdatePriority Priority => new(UpdatePhase.Physics, 99);
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Classes
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

class TrackedSprite
{
    public Transform transform;
    public SpriteRenderer sprite;
    public Collider2D backZone;
    public Collider2D sortbox;
    public float FootY => backZone.bounds.min.y;
    public Dictionary<TrackedSprite, int> orderOverrides = new();
}

class EntityCollision
{
    public Actor actor;
    public Collider2D frontZone;
    public Collider2D backZone;
    public Transform transform;
}
