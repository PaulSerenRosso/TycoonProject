//Interface for non-generic access


using System;

public interface IDependencyEntry
{
    object Resolve(DependenciesManager container);
    Type ServiceType { get; }
    public void Unregister(DependenciesManager container);
}

// Generic implementation - maintains type safety
public class DependencyEntry<T> : IDependencyEntry where T : class
{
    public Type ServiceType => typeof(T);
    
    public object Resolve(DependenciesManager container)
    {
        return container.Resolve<T>(); // Uses generic path internally
    }
    
    public void Unregister(DependenciesManager container)
    {
        container.Unregister<T>(); // Uses generic path internally
    }
}
