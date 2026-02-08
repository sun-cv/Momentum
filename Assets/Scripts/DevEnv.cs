



public class DevEnv : RegisteredService, IServiceTick
{
    readonly Logger Log = Logging.For(LogSystem.System);

    public Hero hero;
    public bool triggered = false;
    
    public override void Initialize()
    {
        Logging.For(LogSystem.Engine)           .SetLevel(LogLevel.Debug);
        Logging.For(LogSystem.Actors)           .SetLevel(LogLevel.Debug);
        Logging.For(LogSystem.Equipment)        .SetLevel(LogLevel.Debug);
        Logging.For(LogSystem.Weapons)          .SetLevel(LogLevel.Debug);
        Logging.For(LogSystem.Animation)        .SetLevel(LogLevel.Debug);
        Logging.For(LogSystem.Movement)         .SetLevel(LogLevel.Trace);
        Logging.For(LogSystem.Hitboxes)         .SetLevel(LogLevel.Debug);
        Logging.For(LogSystem.Combat)           .SetLevel(LogLevel.Debug);



        hero        = HeroFactory.Create();

        hero.Equipment.Equip(new Sword());
        hero.Equipment.Equip(new Shield());
        hero.Equipment.Equip(new Dash());

        Services.Get<CameraRig>().SetCameraTarget(new HeroCameraTarget(){ Hero = hero });
        Services.Get<CameraRig>().ActivateBehavior(CameraBehavior.MouseOffset);
        Services.Get<CameraRig>().ActivateBehavior(CameraBehavior.PlayerDeadzone);
        
        triggered   = true;

    }


    public void Tick()
    {
        Logging.For(LogSystem.Hero).Debug("Health", () => hero.Health);
        Logging.For(LogSystem.Hero).Debug("Parry", () => hero.Parrying);
        Logging.For(LogSystem.Hero).Debug("block", () => hero.Blocking);
    }

    public void Loop()
    {
        
    }


    public UpdatePriority Priority => ServiceUpdatePriority.DevEnv;
}