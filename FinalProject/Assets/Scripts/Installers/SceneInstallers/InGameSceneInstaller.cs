using System;
using Services;
using UnityEngine;

public class InGameSceneInstaller : SceneInstaller
{
    public override void Install(DependenciesManager container)
    {
        InGameService inGameService = container.CreateWithConstructorInjection<InGameService>();
        container.Register<IInGameService>(inGameService);
        
    }
}
