using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

internal class CoroutineRunner : MonoBehaviour
{
    public static CoroutineRunner Instance { get; private set; }

    public static void Spawn()
    {
        if (Instance != null)
        {
            return;
        }

        new GameObject($"{MyPluginInfo.PLUGIN_NAME} {nameof(CoroutineRunner)}", [typeof(CoroutineRunner)]);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(this);
    }

    public static Coroutine Start(IEnumerator routine)
    {
        if (Instance == null)
            Spawn();

        return Instance?.StartCoroutine(routine) ?? null;
    }

    public static void Stop(IEnumerator routine)
    {
        if (Instance == null)
            return;

        Instance.StopCoroutine(routine);
    }

    public static void Stop(Coroutine routine)
    {
        if (Instance == null)
            return;

        Instance.StopCoroutine(routine);
    }
}
