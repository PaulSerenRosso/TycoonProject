using Enums;

namespace Services
{
    public class InGameService : IInGameService
    {
        public InGameService(IInputService inputService)
        {
            inputService.SetEnabledMap( true, EInputMap.Player);
        }
    }
}