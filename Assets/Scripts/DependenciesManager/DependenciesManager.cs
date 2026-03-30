
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Logger;
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
                    GameObject go = new GameObject("DependenciesManager");
                    _instance = go.AddComponent<DependenciesManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    #region Registration (Generic - No Boxing)
    public void Register<T>(T dependency) where T : class
    {
        DependencyStorage<T>.Instance = dependency;
        DependencyStorage<T>.IsRegistered = true;
        _typeMap[typeof(T)] = new DependencyEntry<T>();
    }

    public void RegisterFactory<T>(Func<T> factory) where T : class
    {
        DependencyStorage<T>.Factory = factory;
        DependencyStorage<T>.IsRegistered = true;
        _typeMap[typeof(T)] = new DependencyEntry<T>();
    }

    public void RegisterTransient<T>() where T : class
    {
        DependencyStorage<T>.Factory = () => CreateWithConstructorInjection<T>();
        DependencyStorage<T>.IsRegistered = true;
        _typeMap[typeof(T)] = new DependencyEntry<T>();
    }
    
    public void Unregister<T>()
    {
        DependencyStorage<T>.Instance = default(T);
        DependencyStorage<T>.Factory = null;
        DependencyStorage<T>.IsRegistered = false;
        _typeMap.Remove(typeof(T));
    }

    protected virtual void OnDestroy()
    {
        UnregisterAll();
        if (_instance == this)
        {
            _instance = null;
        }
    }
    
    /// <summary>
    /// Unregisters all dependencies (useful for testing)
    /// </summary>
    public void UnregisterAll()
    {
        // Get all types before clearing the map
        var entries = _typeMap.Values.ToArray();
    
        // Clear the map first
        _typeMap.Clear();
    
        // Then unregister each type through the generic path
        foreach (var entry in entries)
        {
            entry.Unregister(this);
        }
    }
    
    #endregion

    #region Resolution (Generic - No Boxing)
    public T Resolve<T>() where T : class
    {
        if (!DependencyStorage<T>.IsRegistered)
        {
            Log.Default.Log(new LogEntry(LogLevel.Error, $"Dependency of type {typeof(T)} not registered", "DependenciesManager"));
            return null;
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
        Log.Default.Log(new LogEntry(LogLevel.Error, $"Dependency of type {typeof(T)} has no implementation", "DependenciesManager"));
        return null;
    }

    public bool TryResolve<T>(out T dependency) where T : class
    {
        dependency = null;

        if (!DependencyStorage<T>.IsRegistered)
            return false;


        dependency = Resolve<T>();
        if (dependency != null)
        {
            return true;
        }

        return false;
    }

    #endregion

    #region Constructor Injection (Optimized)
    private T CreateWithConstructorInjection<T>() where T : class
    {
        var type = typeof(T);
        var constructors = type.GetConstructors();
        
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
                Log.Default.Log(new LogEntry(LogLevel.Error, $"Cannot resolve parameter {parameters[i].Name} of type {paramType} for {type}", "DependenciesManager"));
                return default(T);
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
               
                    var value = entry.Resolve(this);
                    if (value != null)
                    {
                        property.SetValue(target, value);
                    }
                    else
                    {
                        var injectAttribute = property.GetCustomAttribute<InjectAttribute>();
                        if (!injectAttribute.Optional)
                        {
                            Debug.LogError($"Failed to inject {property.Name} in {target.name}");
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
                var value = entry.Resolve(this);
                if (value != null)
                {
                    field.SetValue(target, value);
                }
                else
                {
                    var injectAttribute = field.GetCustomAttribute<InjectAttribute>();
                    if (!injectAttribute.Optional)
                    {
                        Debug.LogError($"Failed to inject {field.Name} in {target.name}");
                    }
                }
            }
        }
    }
    

    #endregion
}
