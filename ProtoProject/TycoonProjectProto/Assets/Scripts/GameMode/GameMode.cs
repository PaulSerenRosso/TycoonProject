using System;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class GameMode : MonoBehaviour
{
  [SerializeField] protected PlayerPawn playerPawnPrefab;
  [SerializeField] protected PlayerController playerControllerPrefab;
  [SerializeField] protected CameraManager cameraManagerPrefab;
  [SerializeField] protected PlayerStart playerStart;
  
  protected PlayerPawn playerPawn;
  protected PlayerController playerController;
  protected CameraManager cameraManager;

  protected virtual void Awake()
  { 
    playerController =  Instantiate(playerControllerPrefab);
  playerPawn =  Instantiate(playerPawnPrefab, playerStart.transform.position, playerStart.transform.rotation );
  cameraManager = Instantiate(cameraManagerPrefab);
  playerPawn.SetCamera(cameraManager);
  playerController.Possess(playerPawn);
  }
}
