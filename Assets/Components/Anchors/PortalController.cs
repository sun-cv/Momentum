using UnityEngine;



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Controller
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PortalController : Controller
{
    public PortalAnchor Anchor                { get; internal set; }
    
    [SerializeField] private string Name;
    [SerializeField] private string Location;
    [SerializeField] private string Region;

        // REWORK REQUIRED REMOVE NULL-COALESCING
    public void Start()
    {
        Anchor =  new PortalAnchor(new Portal() { Name = Name, Location = Location ??= null, Region = Region ??= null}, gameObject);

        Services.Get<TeleportService>().Register(Anchor);
    }

    public void OnDestroy()
    {
        if (Anchor == null) 
            return;

        Services.Get<TeleportService>().Deregister(Anchor);

        Anchor = null;
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                          Anchor
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PortalAnchor : Anchor
{
    public Portal Owner        { get; init; }

        // -----------------------------------

    public Collider2D Zone     { get; init; }

    // ===============================================================================

    public PortalAnchor(Portal portal, GameObject view)
    {
        Owner           = portal;
        View            = view;

        Zone            = view.GetComponent<Collider2D>();

        Owner.Anchor    = this;
    }
}


