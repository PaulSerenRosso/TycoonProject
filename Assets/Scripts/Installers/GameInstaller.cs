using UnityEngine;

// Project-level installer (persists across scenes)
public abstract class GameInstaller : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // Find and execute all project installers from Resources
        var installers = Resources.LoadAll<GameInstaller>("Installers");
        foreach (var installer in installers)
        {
            var instance = Instantiate(installer);
            DontDestroyOnLoad(instance.gameObject);
            instance.Install(DependenciesManager.Instance);
        }
    }

    public abstract void Install(DependenciesManager container);
}