using System.Linq;
using UnityEngine;





public class DevEnv : RegisteredService, IServiceTick
{
    
    public Hero hero;
    public bool triggered = false;
    
    public override void Initialize()
    {
        Debug.Log("Initializing dev env");

        Log.Configure((config) => config
        .System(LogSystem.Engine, LogLevel.Debug)
        .System(LogSystem.Weapon, LogLevel.Trace)
        .System(LogSystem.Movement, LogLevel.Debug)
        .System(LogSystem.Physics, LogLevel.Debug)
        .System(LogSystem.Equipment, LogLevel.Trace));

    }


    public void Tick()
    {

        if (!triggered)
        {
            hero        = HeroFactory.Create();
            Services.Get<CameraRig>().SetCameraTarget(new HeroCameraTarget(){ Hero = hero });
            Services.Get<CameraRig>().ActivateBehavior(CameraBehavior.MouseOffset);
            Services.Get<CameraRig>().ActivateBehavior(CameraBehavior.PlayerDeadzone);
            triggered   = true;
        }

    }

    public void Loop()
    {
        
    }


    public UpdatePriority Priority => ServiceUpdatePriority.DevEnv;
}