using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using Momentum.Interface;
using Momentum.Actor.Hero;


namespace Momentum.Cameras
{


    // BUG FIX - If following, offset continues to calculate based on intended direction while in a forced stationary position. Causes wobbly camera.

public class CameraRig : MonoBehaviour
{
    [SerializeField] public new Camera                     camera;
    [SerializeField] public Transform                      cameraTarget;
    [SerializeField] public CinemachineCamera              cinemachineCamera;
    [SerializeField] public List<MonoBehaviour>            behaviorComponents;

    readonly List<ICameraBehavior> activeBehaviors  = new();

    CameraContext context;

    void Awake()
    {
        var composer = cinemachineCamera.GetComponent<CinemachinePositionComposer>();

        context = new CameraContext
        {
            camera              = camera,
            cameraTarget        = cameraTarget,
            cinemachineCamera   = cinemachineCamera,
            composer            = composer,
            hero                = null
        };

    }

    public void Initialize(HeroContext hero)
    {
        context.hero = hero;

        foreach (var component in behaviorComponents)
        {
            if (component is ICameraBehavior behavior)
            {
                behavior.Initialize(context);
                activeBehaviors.Add(behavior);
            }
        }
    }

    public void Tick()
    {
        TrackPlayer();

        foreach (var behavior in activeBehaviors)
        {
            behavior.Tick();
        }
    }

    public void TickLate()
    {
        foreach (var behavior in activeBehaviors)
        {
            behavior.TickLate();
        }
    }

    public void TickFixed()
    {
        //noop
    }

    public void EnableBehavior<T>() where T : ICameraBehavior
    {
        foreach (var component in behaviorComponents)
        {
            if (component is T behavior && !activeBehaviors.Contains(behavior))
            {
                behavior.Initialize(context);
                activeBehaviors.Add(behavior);
                break;
            }
        }
    }

    public void DisableBehavior<T>() where T : ICameraBehavior
    {
        activeBehaviors.RemoveAll(b => b is T);
    }

    public bool IsBehaviorEnabled<T>() where T : ICameraBehavior
    {
        return activeBehaviors.Exists(b => b is T);
    }

    void TrackPlayer()
    {
        cameraTarget.position = context.hero.transform.position;
    }
}

}