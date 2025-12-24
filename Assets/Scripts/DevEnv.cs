using System.Linq;
using UnityEngine;





public class DevEnv : RegisteredService, IServiceTick
{
    
    public HeroController controller;
    public bool triggered;
    
    public override void Initialize()
    {
        Debug.Log("Initializing dev env");

        controller = HeroFactory.Create();


    }

    public void Tick()
    {
        

    }



    public UpdatePriority Priority => ServiceUpdatePriority.DevEnv;
}