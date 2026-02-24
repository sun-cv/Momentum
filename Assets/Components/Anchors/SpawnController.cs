using UnityEngine;



public class SpawnController : Controller
{
    [SerializeField] private string Definition;

        // -----------------------------------

    public SpawnAnchor Anchor       { get; set; }

    // ===============================================================================

    public void Start()
    {
        Anchor =  new SpawnAnchor(new SpawnPoint() { Name  = Definition }, gameObject);

        Services.Get<SpawnerService>().Register(Anchor.Owner);
    }

        // REWORK REQUIRED - FIX DEREGISTER
    public void OnDestroy()
    {
        if (Anchor == null) 
            return;

        // Services.Get<SpawnerService>().Deregister(Anchor.Owner.Spawner);

        Anchor = null;
    }
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                          Anchor
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class SpawnAnchor : Anchor
{
    public SpawnPoint Owner         { get; init; }

        // -----------------------------------

    public Collider2D Zone          { get; init; }

    // ===============================================================================

    public SpawnAnchor(SpawnPoint portal, GameObject view)
    {
        Owner           = portal;
        View            = view;
        Zone            = view.GetComponent<Collider2D>();

        Owner.Anchor    = this;
    }
}


