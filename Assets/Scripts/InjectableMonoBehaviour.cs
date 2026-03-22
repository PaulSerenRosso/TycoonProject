using UnityEngine;

// Base class for MonoBehaviours that need DI
/*
public abstract class InjectableMonoBehaviour : MonoBehaviour
{
    protected virtual void Awake()
    {
        ServiceContainer.Instance.InjectProperties(this);
        ServiceContainer.Instance.InjectFields(this);
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
            ServiceContainer.Instance.InjectProperties(component);
            ServiceContainer.Instance.InjectFields(component);
        }
    }
}
*/