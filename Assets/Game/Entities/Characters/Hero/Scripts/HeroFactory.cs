




public static class HeroFactory
{
    public static Hero Create(HeroDefinition data = null)
    {
        data          ??= new HeroDefinition();

        var prefab      = Registry.Prefabs.Get("HeroController");
        var instance    = UnityEngine.Object.Instantiate(prefab);
        var controller  = instance.GetComponent<HeroController>();

        var hero        = new Hero();
        hero.Initialize(controller, data);

        hero.Equipment.Equip(new Sword());
        hero.Equipment.Equip(new Shield());
        hero.Equipment.Equip(new Dash());
                
        return hero;
    }
}
