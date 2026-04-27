using UnityEngine;

namespace Camera
{
    public class CameraManagerPlay : CameraManager
    {
      [SerializeField] [Range(0.0f,90.0f)] private float angle;
      [SerializeField] private float startOffsetY;
      [SerializeField] private float startOffsetZ;
      

      public void SetPlayerPosition(PlayerPawn playerPawn)
      {
          transform.rotation = Quaternion.Euler(angle, 0, 0);
         transform.position = playerPawn.transform.position + new Vector3(0, startOffsetY, startOffsetZ);
      }
    }
}