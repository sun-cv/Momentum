using UnityEngine;
using Unity.Cinemachine;
using Momentum.Actor.Hero;
using Momentum.Interface;


namespace Momentum.Cameras
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