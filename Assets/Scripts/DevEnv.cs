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
    }

    public void Tick()
    {
        

    }



    public UpdatePriority Priority => ServiceUpdatePriority.DevEnv;
}