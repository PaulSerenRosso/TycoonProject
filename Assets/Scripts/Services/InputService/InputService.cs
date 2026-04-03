using Enums;

namespace Services
{
    public class InputService : IInputService
    {
        private TycoonProjectInputActions inputActions;
        
        public InputService()
        {
            inputActions = new TycoonProjectInputActions();
        }


        public void SetEnabledMap(bool enabled, EInputMap map = EInputMap.Player)
        {
           
            switch (map)
            {
                case EInputMap.Player:
                {
                    if(enabled)
                    inputActions.Player.Enable();
                    else
                        inputActions.Player.Disable();
                    break;
                }
            }
        }
    }
}