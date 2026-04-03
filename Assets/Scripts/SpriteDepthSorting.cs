using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public enum SortTier { Ground, Prop, Entity }


public class SpriteLayeringSystem : RegisteredService, IServiceStep, IBind
{
    readonly List<TrackedSprite> trackedSprites = new();
    
    // ===============================================================================

    public void Register(Bridge bridge)
    {
        var tracked = new TrackedSprite 
        { 
            Tier        = bridge.Owner.Definition.Rendering.DepthSortingTier,
            Transform   = bridge.Body.transform, 
            Sprite      = bridge.Sprite,
            Sortbox     = bridge.Sortbox
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
        
        foreach (var actor in Actors.GetInterface<IDepthSorted>())
        {
            Register(actor.Bridge);
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

                expanded.Sortbox.bounds.Expand(Config.Rendering.SPRITE_OVERLAP_LOOKAHEAD);

                bool overlapping = original.Sortbox.bounds.Intersects(sprite2.Sortbox.bounds);
                bool approaching = expanded.Sortbox.bounds.Intersects(sprite2.Sortbox.bounds);

                if (overlapping || approaching)
                {
                    if (!sprite1.OrderOverrides.ContainsKey(sprite2))
                    {
                        float footY1 = sprite1.FootY;
                        float footY2 = sprite2.FootY;

                        if (footY1 < footY2)
                        {
                            if (sprite1.Tier <= sprite2.Tier)
                                sprite1.OrderOverrides[sprite2] =  1;

                            sprite2.OrderOverrides[sprite1] = -1;
                        }
                        else
                        {
                            sprite1.OrderOverrides[sprite2] = -1;
                            sprite2.OrderOverrides[sprite1] =  1;
                        }
                    }
                }
                else
                {
                    sprite1.OrderOverrides.Remove(sprite2);
                    sprite2.OrderOverrides.Remove(sprite1);
                }
            }
        }

        foreach (var tracked in trackedSprites)
        {
            int finalOrder;

            if (tracked.OrderOverrides.Count > 0)
            {
                finalOrder = 0;

                foreach (var offset in tracked.OrderOverrides.Values)
                {
                    finalOrder += offset;
                }
            }
            else
            {
                finalOrder = Mathf.RoundToInt(-tracked.FootY * 100);
            }

            tracked.Sprite.sortingOrder = finalOrder;
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
        trackedSprites.Clear();
    }

    public UpdatePriority Priority => ServiceUpdatePriority.SpriteLayering;
}



public class DepthCollisionSystem : RegisteredService, IServiceStep, IBind
{
    
    readonly List<EntityCollision> trackedSprites = new();

        // ===============================================================================

    public void Register(Bridge bridge)
    {
        trackedSprites.Add(new EntityCollision
        {
            Actor       = bridge.Owner,
            FrontZone   = bridge.FrontZone,
            BackZone    = bridge.BackZone,
            Transform   = bridge.Body.transform
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
        
        foreach (var actor in Actors.GetInterface<IDepthColliding>())
        {
            Register(actor.Bridge);
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
                
                float distance  = Vector2.Distance(entity1.Transform.position, entity2.Transform.position);
                
                if (distance > 5f) 
                {   
                    continue;
                }

                float y1 = entity1.Transform.position.y;
                float y2 = entity2.Transform.position.y;
                
                if (y1 < y2)
                {
                    Physics2D.IgnoreCollision(entity1.BackZone, entity2.FrontZone, false);
                    Physics2D.IgnoreCollision(entity1.FrontZone, entity2.FrontZone, true);
                    Physics2D.IgnoreCollision(entity1.FrontZone, entity2.BackZone, true);
                    Physics2D.IgnoreCollision(entity1.BackZone, entity2.BackZone, true);
                }
                else
                {
                    Physics2D.IgnoreCollision(entity2.BackZone, entity1.FrontZone, false);
                    Physics2D.IgnoreCollision(entity1.FrontZone, entity2.FrontZone, true);
                    Physics2D.IgnoreCollision(entity1.BackZone, entity2.FrontZone, true);
                    Physics2D.IgnoreCollision(entity1.BackZone, entity2.BackZone, true);
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
        trackedSprites.Clear();
    }

    public UpdatePriority Priority => ServiceUpdatePriority.SpriteDepthSorting;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Classes
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

class TrackedSprite
{
    public SortTier Tier                                    { get; set; }
    public Transform Transform                              { get; set; }
    public SpriteRenderer Sprite                            { get; set; }
    public Collider2D BackZone                              { get; set; }
    public Collider2D Sortbox                               { get; set; }
    public Dictionary<TrackedSprite, int> OrderOverrides    { get; set; } = new();
        
    public float FootY => Sortbox.bounds.min.y;
}

class EntityCollision
{
    public Actor Actor                                      { get; set; }
    public Collider2D FrontZone                             { get; set; }
    public Collider2D BackZone                              { get; set; }
    public Transform Transform                              { get; set; }
}
