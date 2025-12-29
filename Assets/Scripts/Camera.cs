using UnityEngine;
using Unity.Cinemachine;





public class CameraRig : RegisteredService, IServiceTick, IServiceLoop
{
    Camera                      cameraRoot;
    Transform                   cameraTarget;
    CinemachineCamera           camera;
    CinemachinePositionComposer composer;

    Context context;

    public override void Initialize()
    {
        var rig  = new GameObject("CameraRig");
        var core = new GameObject("Camera");
        var root = new GameObject("CameraRoot");
        var hero = new GameObject("CameraTarget");

        core.transform.SetParent(rig.transform, false);
        root.transform.SetParent(rig.transform, false);
        hero.transform.SetParent(rig.transform, false);

        camera     = core.AddComponent<CinemachineCamera>();
        composer   = core.AddComponent<CinemachinePositionComposer>();

        cameraRoot = root.AddComponent<Camera>();
                     root.AddComponent<CinemachineBrain>();

        cameraRoot.orthographic         = true;
        camera.Lens.OrthographicSize    = 270/32;

        cameraTarget                    = hero.transform;
        camera.Target.TrackingTarget    = cameraTarget;

    }

    public void Tick()
    {
        
    }

    public void Loop()
    {
        
    }


    public void AssignHero(Hero hero)   { context = hero.Context;}

    public UpdatePriority Priority => ServiceUpdatePriority.CameraRig;
}