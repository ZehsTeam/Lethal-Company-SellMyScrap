using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

internal class CoroutineRunner : MonoBehaviour
{
    public static CoroutineRunner Instance { get; private set; }

    public static CoroutineRunner Spawn()
    {
        if (Instance != null)
        {
            return Instance;
        }

        var gameObject = new GameObject($"{MyPluginInfo.PLUGIN_NAME} {nameof(CoroutineRunner)}", [typeof(CoroutineRunner)])
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        DontDestroyOnLoad(gameObject);

        return gameObject.GetComponent<CoroutineRunner>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static Coroutine Start(IEnumerator routine)
    {
        if (Instance == null)
        {
            return Spawn()?.StartCoroutine(routine) ?? null;
        }

        return Instance?.StartCoroutine(routine) ?? null;
    }

    public static void Stop(IEnumerator routine)
    {
        Instance?.StopCoroutine(routine);
    }

    public static void Stop(Coroutine routine)
    {
        Instance?.StopCoroutine(routine);
    }
}
