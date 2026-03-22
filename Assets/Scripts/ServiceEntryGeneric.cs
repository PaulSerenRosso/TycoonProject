//Interface for non-generic access

/*
public interface IServiceEntry
{
    object Resolve(ServiceContainer container);
    Type ServiceType { get; }
}

// Generic implementation - maintains type safety
public class ServiceEntry<T> : IServiceEntry
{
    public Type ServiceType => typeof(T);
    
    public object Resolve(ServiceContainer container)
    {
        return container.Resolve<T>(); // Uses generic path internally
    }
}
*/