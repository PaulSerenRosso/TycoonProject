using System.ComponentModel.Design;
using UnityEngine;

// Base class for MonoBehaviours that need DI

public abstract class InjectableMonoBehaviour : MonoBehaviour
{
    protected virtual void Awake()
    {
        DependenciesManager.Instance.InjectProperties(this);
        DependenciesManager.Instance.InjectFields(this);
    }
}

// Alternative: Auto-injection component
public class AutoInject : MonoBehaviour
{
    private void Awake()
    {
        var components = GetComponents<MonoBehaviour>();
        foreach (var component in components)
        {
            DependenciesManager.Instance.InjectProperties(component);
            DependenciesManager.Instance.InjectFields(component);
        }
    }
}
