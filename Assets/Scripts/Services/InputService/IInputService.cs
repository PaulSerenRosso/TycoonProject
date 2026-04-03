using BaseClasses;
using Enums;

namespace Services
{
    public interface IInputService : IService
    {
        void SetEnabledMap( bool enabled,EInputMap map = EInputMap.Player);
    }
}