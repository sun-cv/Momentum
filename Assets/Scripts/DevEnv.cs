



public class DevEnv : RegisteredService, IServiceTick
{
    
    public Hero hero;
    public bool triggered = false;
    
    public override void Initialize()
    {
        Log.Configure((config) => config
        .Global(LogLevel.Error)
        .System(LogSystem.Engine,       LogLevel.Debug)
        .System(LogSystem.Weapon,       LogLevel.Trace)
        .System(LogSystem.Movement,     LogLevel.Debug)
        .System(LogSystem.Equipment,    LogLevel.Debug)
        .System(LogSystem.Animation,    LogLevel.Trace)
        // .System(LogSystem.Effects,      LogLevel.Trace)
        .System(LogSystem.Hero,         LogLevel.Debug));
    }


    public void Tick()
    {


        if (!triggered)
        {
            hero        = HeroFactory.Create();

            hero.Equipment.Equip(new Sword());
            hero.Equipment.Equip(new Shield());
            hero.Equipment.Equip(new Dash());

            Services.Get<CameraRig>().SetCameraTarget(new HeroCameraTarget(){ Hero = hero });
            Services.Get<CameraRig>().ActivateBehavior(CameraBehavior.MouseOffset);
            Services.Get<CameraRig>().ActivateBehavior(CameraBehavior.PlayerDeadzone);
            
            triggered   = true;
        }


        Log.Debug(LogSystem.Hero, LogCategory.State, "Hero State", "Parry", () => hero.Parrying);
        Log.Debug(LogSystem.Hero, LogCategory.State, "Hero State", "block", () => hero.Blocking);

    }

    public void Loop()
    {
        
    }


    public UpdatePriority Priority => ServiceUpdatePriority.DevEnv;
}