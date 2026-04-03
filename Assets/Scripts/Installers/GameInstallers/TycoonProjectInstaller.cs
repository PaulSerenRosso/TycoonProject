

using Core.Events;
using Services;

namespace Installers.GameInstallers
{
    public class TycoonProjectInstaller : GameInstaller
    {
        public override void Install(DependenciesManager container)
        {
            EventBus eventBus = new EventBus();
            container.Register(eventBus);
            container.Register<IInputService>(new InputService());
        }
    }
}