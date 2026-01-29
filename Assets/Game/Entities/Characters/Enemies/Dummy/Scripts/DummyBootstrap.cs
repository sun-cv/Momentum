public class DummyBootstrap : Controller
{
    private Dummy dummy;

    public void Start()
    {
        if (TryGetComponent<BridgeController>(out var bridgeController) && bridgeController.Bridge != null)
        {
            dummy = (Dummy)bridgeController.Bridge.Owner;
        }
        else
        {
            dummy = DummyFactory.Create(gameObject);
        }
    }
}
