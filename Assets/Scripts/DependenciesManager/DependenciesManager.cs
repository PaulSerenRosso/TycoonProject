
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


public class DependenciesManager : MonoBehaviour
{
    private static DependenciesManager _instance;
    
    // Generic storage - no boxing!
    private static class DependencyStorage<T>
    {
        public static T Instance;
        public static Func<T> Factory;
        public static bool IsRegistered;
    }

    // Non-generic lookup for reflection scenarios
    private Dictionary<Type, IDependencyEntry> _typeMap = new Dictionary<Type, IDependencyEntry>();

    public static DependenciesManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DependenciesManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ServiceContainer");
                    _instance = go.AddComponent<DependenciesManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    #region Registration (Generic - No Boxing)
    public void Register<T>(T service)
    {
        DependencyStorage<T>.Instance = service;
        DependencyStorage<T>.IsRegistered = true;
        _typeMap[typeof(T)] = new DependencyEntry<T>();
    }

    public void RegisterFactory<T>(Func<T> factory)
    {
        DependencyStorage<T>.Factory = factory;
        DependencyStorage<T>.IsRegistered = true;
        _typeMap[typeof(T)] = new DependencyEntry<T>();
    }

    public void RegisterTransient<T>() where T : class, new()
    {
        DependencyStorage<T>.Factory = () => CreateWithConstructorInjection<T>();
        DependencyStorage<T>.IsRegistered = true;
        _typeMap[typeof(T)] = new DependencyEntry<T>();
    }

    public void RegisterTransient<TInterface, TImplementation>() 
        where TImplementation : class, TInterface, new()
    {
        DependencyStorage<TInterface>.Factory = () => CreateWithConstructorInjection<TImplementation>();
        DependencyStorage<TInterface>.IsRegistered = true;
        _typeMap[typeof(TInterface)] = new DependencyEntry<TInterface>();
    }
    #endregion

    #region Resolution (Generic - No Boxing)
    public T Resolve<T>()
    {
        if (!DependencyStorage<T>.IsRegistered)
        {
            throw new InvalidOperationException($"Service of type {typeof(T)} not registered");
        }

        // Singleton
        if (DependencyStorage<T>.Instance != null)
        {
            return DependencyStorage<T>.Instance;
        }

        // Factory
        if (DependencyStorage<T>.Factory != null)
        {
            return DependencyStorage<T>.Factory();
        }

        throw new InvalidOperationException($"Service of type {typeof(T)} has no implementation");
    }

    public bool TryResolve<T>(out T service)
    {
        service = default(T);
        
        if (!DependencyStorage<T>.IsRegistered)
            return false;

        try
        {
            service = Resolve<T>();
            return true;
        }
        catch
        {
            return false;
        }
    }
    #endregion

    #region Constructor Injection (Optimized)
    private T CreateWithConstructorInjection<T>() where T : new()
    {
        var type = typeof(T);
        var constructors = type.GetConstructors();
        
        // Try parameterless constructor first (fastest path)
        var parameterlessConstructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);
        if (parameterlessConstructor != null)
        {
            return new T();
        }

        // Fall back to DI constructor
        var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
        var parameters = constructor.GetParameters();
        var args = new object[parameters.Length]; // Only boxing here when absolutely necessary

        for (int i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            if (_typeMap.TryGetValue(paramType, out IDependencyEntry entry))
            {
                args[i] = entry.Resolve(this);
            }
            else if (parameters[i].HasDefaultValue)
            {
                args[i] = parameters[i].DefaultValue;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Cannot resolve parameter {parameters[i].Name} of type {paramType} for {type}");
            }
        }

        return (T)Activator.CreateInstance(type, args);
    }
    #endregion

    #region Property Injection (Cached Reflection)
    private static readonly Dictionary<Type, PropertyInfo[]> _propertyCache = 
        new Dictionary<Type, PropertyInfo[]>();
    private static readonly Dictionary<Type, FieldInfo[]> _fieldCache = 
        new Dictionary<Type, FieldInfo[]>();

    public void InjectProperties(MonoBehaviour target)
    {
        var type = target.GetType();
        
        if (!_propertyCache.TryGetValue(type, out PropertyInfo[] properties))
        {
            properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<InjectAttribute>() != null && p.CanWrite)
                .ToArray();
            _propertyCache[type] = properties;
        }

        foreach (var property in properties)
        {
            if (_typeMap.TryGetValue(property.PropertyType, out IDependencyEntry entry))
            {
                try
                {
                    var value = entry.Resolve(this);
                    property.SetValue(target, value);
                }
                catch (InvalidOperationException ex)
                {
                    var injectAttribute = property.GetCustomAttribute<InjectAttribute>();
                    if (!injectAttribute.Optional)
                    {
                        Debug.LogError($"Failed to inject {property.Name} in {target.name}: {ex.Message}");
                    }
                }
            }
        }
    }

    public void InjectFields(MonoBehaviour target)
    {
        var type = target.GetType();
        
        if (!_fieldCache.TryGetValue(type, out FieldInfo[] fields))
        {
            fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.GetCustomAttribute<InjectAttribute>() != null)
                .ToArray();
            _fieldCache[type] = fields;
        }

        foreach (var field in fields)
        {
            if (_typeMap.TryGetValue(field.FieldType, out IDependencyEntry entry))
            {
                try
                {
                    var value = entry.Resolve(this);
                    field.SetValue(target, value);
                }
                catch (InvalidOperationException ex)
                {
                    var injectAttribute = field.GetCustomAttribute<InjectAttribute>();
                    if (!injectAttribute.Optional)
                    {
                        Debug.LogError($"Failed to inject {field.Name} in {target.name}: {ex.Message}");
                    }
                }
            }
        }
    }
    #endregion
}
