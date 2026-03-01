public class HeroBootstrap : Controller
{
    private Actor hero;

    public void Start()
    {
        if (TryGetComponent<BridgeController>(out var bridgeController) && bridgeController.Bridge != null)
        {
            hero = (Hero)bridgeController.Bridge.Owner;
        }
        else
        {
            hero = Factories.Get<HeroFactory>().Spawn(gameObject.transform.position);
        }
    }
}
