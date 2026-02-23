using System;



public class DevEnv : RegisteredService, IServiceTick, IServiceLoop, IInitialize
{
    Hero hero;

    FrameTimer Left;
    FrameTimer Right;

    // ===============================================================================
    
    public void Initialize()
    {
        Services.Lane.Register(this);

        hero = HeroFactory.Create();

        hero.Equipment.Equip(new Sword());
        hero.Equipment.Equip(new Shield());
        hero.Equipment.Equip(new Dash());

        Services.Get<CameraRig>().SetCameraTarget(new ActorCameraTarget(hero));
        Services.Get<CameraRig>().ActivateBehavior(CameraBehavior.MouseOffset);
        Services.Get<CameraRig>().ActivateBehavior(CameraBehavior.PlayerDeadzone);
        
        DebugLogSetup();

        Left        = new(60);
        Right       = new(60);

        Left.OnTimerStop += () => Emit.Global<Message<Request, TeleportEvent>>(new(Guid.NewGuid(), Request.Trigger, new() { Name = "Left", Location = "Home", Agent = hero}));
        Left.OnTimerStop += () => Right.Restart();

        Right.OnTimerStop += () => Emit.Global<Message<Request, TeleportEvent>>(new(Guid.NewGuid(), Request.Trigger, new() { Name = "Right", Location = "Home", Agent = hero}));
        Right.OnTimerStop += () => Left.Restart();

        Left.Start();
    }

    // ===============================================================================

    public void Tick()
    {

    }

    public void Loop()
    {

    }

    // ===============================================================================


    readonly Logger Log = Logging.For(LogSystem.Dev);

    public void DebugLogSetup()
    {
        Logging.For(LogSystem.Dev)              .SetLevel(LogLevel.Debug);
        Logging.For(LogSystem.Engine)           .SetLevel(LogLevel.Debug);
        Logging.For(LogSystem.Actors)           .SetLevel(LogLevel.Debug);
        Logging.For(LogSystem.Equipment)        .SetLevel(LogLevel.Debug);
        Logging.For(LogSystem.Weapons)          .SetLevel(LogLevel.Debug);
        Logging.For(LogSystem.Animation)        .SetLevel(LogLevel.Trace);
        Logging.For(LogSystem.Movement)         .SetLevel(LogLevel.Trace);
        Logging.For(LogSystem.Hitboxes)         .SetLevel(LogLevel.Debug);
        Logging.For(LogSystem.Combat)           .SetLevel(LogLevel.Debug);
        Logging.For(LogSystem.Presence)         .SetLevel(LogLevel.Debug);
        Logging.For(LogSystem.Lifecycle)        .SetLevel(LogLevel.Debug);
        Logging.For(LogSystem.Hero)             .SetLevel(LogLevel.Debug);    
    }

    public void DebugLog()
    {
        Logging.For(LogSystem.Hero).Debug("Health",() => hero.Health);
        Logging.For(LogSystem.Hero).Debug("Parry", () => hero.Parrying);
        Logging.For(LogSystem.Hero).Debug("block", () => hero.Blocking);
    }

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.DevEnv;
}