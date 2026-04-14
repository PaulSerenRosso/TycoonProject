// Scene-level installer (per scene)

using UnityEngine;

public abstract class SceneInstaller : MonoBehaviour
{
    private DependenciesManager _container;

    protected virtual void Awake()
    {
        _container = DependenciesManager.Instance;
        Install(_container);
    }

    protected virtual void OnDestroy()
    {
        Uninstall(_container);
    }
    
    public abstract void Install(DependenciesManager container);
    public virtual void Uninstall(DependenciesManager container) { }
}