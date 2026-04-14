using UnityEngine;

// Project-level installer (persists across scenes)
public abstract class GameInstaller : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // Find and execute all project installers from Resources
        var installer = Resources.LoadAll<GameInstaller>("GameInstaller");

        if (installer.Length == 0)
        {
            throw new System.Exception("No GameInstaller found in Resources");
        }

        var instance = Instantiate(installer[0]);
        DontDestroyOnLoad(instance.gameObject);
        instance.Install(DependenciesManager.Instance);
    }

    public abstract void Install(DependenciesManager container);
}