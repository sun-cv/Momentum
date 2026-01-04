using System.Linq;
using UnityEngine;





public class DevEnv : RegisteredService, IServiceTick
{
    
    public Hero hero;
    public bool triggered;
    
    public override void Initialize()
    {
        Debug.Log("Initializing dev env");

        hero = HeroFactory.Create();

        Log.Configure((config) => config
        .System(LogSystem.Weapon, LogLevel.Trace)
        .System(LogSystem.Movement, LogLevel.Trace)
        .System(LogSystem.Effects, LogLevel.Debug));

    }

    public void Tick()
    {

    }

    public void Loop()
    {
        
    }


    public UpdatePriority Priority => ServiceUpdatePriority.DevEnv;
}