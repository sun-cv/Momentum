using System;
using UnityEngine;



public class SpawnController : Controller
{
    [SerializeField] private string definition;

        // -----------------------------------

    public SpawnAnchor Anchor       { get; set; }

    // ===============================================================================

    public void Start()
    {
        Anchor = new SpawnAnchor(definition, gameObject);

        Emit.Global<Message<Request, SpawnerEvent>>(new(Request.Create, new SpawnerEvent(Anchor.Owner)));
    }

    public void OnDestroy()
    {
        if (Anchor == null) 
            return;

        Emit.Global<Message<Request, SpawnerEvent>>(new(Request.Destroy, new SpawnerEvent(Anchor.Owner)));

        Anchor = null;
    }

    private void OnDrawGizmos()
    {
        if (!SpawnerService.ShowDebugGizmos) return;
        
        Gizmos.color = Settings.Debug.GIZMO_COLOR_SPAWNER;
        DrawColliders();
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!SpawnerService.ShowDebugGizmos) return;

        Gizmos.color = Settings.Debug.GIZMO_COLOR_SPAWNER_SELECTED;        
        DrawColliders();
    }
    
    void DrawColliders()
    {
        var colliders = GetComponentsInChildren<Collider2D>();

        foreach (var collider in colliders)
            GizmoTools.DrawCollider(collider, true);
    }
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                          Anchor
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class SpawnAnchor : Anchor
{
    public SpawnPoint Owner         { get; init; }

        // -----------------------------------

    public Collider2D Area          { get; init; }

    // ===============================================================================

    public SpawnAnchor(string definition, GameObject view)
    {
        View            = view;
        Area            = view.GetComponent<Collider2D>();

        Owner           = new();

        Owner.Name      = definition;
        Owner.Anchor    = this;
        Owner.Area      = Area;
    }
}


