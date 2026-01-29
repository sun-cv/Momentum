




public class BridgeController : Controller
{
    public Bridge Bridge                { get; internal set; }
    public void Bind(Bridge bridge)
    {
        if (Bridge != null)
            return;

        Bridge = bridge;

        Entities.Register(Bridge);
    }

    public void OnDestroy()
    {
        if (Bridge == null) 
            return;

        Entities.Deregister(Bridge);
    }
}
