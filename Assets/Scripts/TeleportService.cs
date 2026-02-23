using System.Collections.Generic;
using System.Linq;



public class TeleportService : RegisteredService, IServiceLoop
{

    readonly List<PortalAnchor>  portals  = new();
    readonly List<TeleportEvent> requests = new();

    // ===============================================================================

    public TeleportService()
    {
        binding = Link.Global<Message<Request, TeleportEvent>>(HandleTeleportRequest);
    }

    // ===============================================================================

    public void Loop()
    {
        ProcessTeleportRequests();
    }

    // ===============================================================================

    public void ProcessTeleportRequests()
    {
        foreach (var request in requests)
        {
            Teleport(request.Name, request.Location, request.Agent);
        }
    }

    public void Teleport(string name, string location, Agent agent)
    {
        var portal = GetPortalByNameAndLocation(name, location);
        agent.Bridge.Body.position = portal.View.transform.position;
    }

    public void Register(PortalAnchor anchor)
    {
        portals.Add(anchor);
    }

    public void Deregister(PortalAnchor anchor)
    {
        portals.Remove(anchor);
    }
     
    public void Clear()
    {
        portals.Clear();
    }

    // ===============================================================================
    //  Events
    // ===============================================================================
    
    void HandleTeleportRequest(Message<Request, TeleportEvent> message)
    {
        requests.Add(message.Payload);
    }

    // ===============================================================================

    public PortalAnchor GetPortalByName(string name)
    {
        return portals.FirstOrDefault(anchor => anchor.Owner.Name == name);
    }

    public PortalAnchor GetPortalByNameAndLocation(string name, string location)
    {
        return portals.FirstOrDefault(anchor => anchor.Owner.Name == name && anchor.Owner.Location == location);
    }

    public PortalAnchor GetPortalsByLocation(string location)
    {
        return portals.FirstOrDefault(anchor => anchor.Owner.Location == location);
    }

    public List<PortalAnchor> GetPortalsByRegion(string region)
    {
        return portals.Where(anchor => anchor.Owner.Region == region).ToList();
    }

    // ===============================================================================

    readonly Logger Log = new(LogSystem.Teleport, LogLevel.Debug);

    readonly EventBinding<Message<Request, TeleportEvent>> binding;

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
        
        portals.Clear();
        requests.Clear();

        Link.UnsubscribeGlobal(binding);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.TeleportService;
}


public readonly struct TeleportEvent
{
    public string Name          { get; init; }
    public string Location      { get; init; }
    public Agent Agent          { get; init; }
}
