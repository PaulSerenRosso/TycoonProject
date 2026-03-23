//Interface for non-generic access


using System;

public interface IDependencyEntry
{
    object Resolve(DependenciesManager container);
    Type ServiceType { get; }
}

// Generic implementation - maintains type safety
public class DependencyEntry<T> : IDependencyEntry
{
    public Type ServiceType => typeof(T);
    
    public object Resolve(DependenciesManager container)
    {
        return container.Resolve<T>(); // Uses generic path internally
    }
}
