using UnityEngine;
using Unity.Cinemachine;


namespace Momentum
{

public class CameraContext
{
    public Camera                           camera;                       
    public Transform                        cameraTarget;   
    public CinemachineCamera                cinemachineCamera;
    public CinemachinePositionComposer      composer;
    public HeroContext                      hero;         
}
}