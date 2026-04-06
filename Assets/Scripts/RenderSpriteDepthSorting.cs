using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class DepthCollisionSystem : RegisteredService, IServiceStep, IBind
{
    
    readonly List<EntityDepthCollision> trackedSprites = new();

        // ===============================================================================

    public void Register(Bridge bridge)
    {
        trackedSprites.Add(new EntityDepthCollision
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

class EntityDepthCollision
{
    public Actor Actor                                      { get; set; }
    public Collider2D FrontZone                             { get; set; }
    public Collider2D BackZone                              { get; set; }
    public Transform Transform                              { get; set; }
}
