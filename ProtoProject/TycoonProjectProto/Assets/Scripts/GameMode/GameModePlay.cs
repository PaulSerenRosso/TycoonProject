using Camera;

namespace GameManager
{
    public class GameModePlay : GameMode
    {
        private CameraManagerPlay cameraManagerPlay;
        

        protected override void Awake()
        {
            base.Awake();
            cameraManagerPlay = (CameraManagerPlay)cameraManager;
            
            cameraManagerPlay.SetPlayerPosition(playerPawn);
        }
    }
}