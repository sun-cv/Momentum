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
            Tier        = bridge.Owner.Definition.Appearance.DepthSortingTier,
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
                        if (sprite1.Tier != SortTier.Ground && sprite2.Tier == SortTier.Ground)
                        {
                            sprite1.OrderOverrides[sprite2] =  1;
                            sprite2.OrderOverrides[sprite1] = -1;
                        }
                        else if (sprite1.Tier == SortTier.Ground && sprite2.Tier != SortTier.Ground)
                        {
                            sprite1.OrderOverrides[sprite2] = -1;
                            sprite2.OrderOverrides[sprite1] =  1;
                        }
                        else
                        {
                            float footY1 = sprite1.FootY;
                            float footY2 = sprite2.FootY;
                            sprite1.OrderOverrides[sprite2] = footY1 < footY2 ?  1 : -1;
                            sprite2.OrderOverrides[sprite1] = footY1 < footY2 ? -1 :  1;
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
