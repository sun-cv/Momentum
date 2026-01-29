public class HeroBootstrap : Controller
{
    private Hero hero;

    public void Start()
    {
        if (TryGetComponent<BridgeController>(out var bridgeController) && bridgeController.Bridge != null)
        {
            hero = (Hero)bridgeController.Bridge.Owner;
        }
        else
        {
            hero = HeroFactory.Create(gameObject);
        }
    }
}
