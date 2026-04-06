using System.Collections.Generic;
using UnityEngine;




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
