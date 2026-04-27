using System;
using UnityEngine;

public abstract class PlayerPawn : MonoBehaviour
{
    protected CameraManager cameraManager;
 

    public void OnPossess()
    {
       
    }

    public void SetCamera(CameraManager inCameraManager)
    {
       cameraManager = inCameraManager;
    }

    protected virtual void Start()
    {
       
    }
}
