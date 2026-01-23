using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    public bool isDontDestroyOnLoad = true;

    private static T instance;

    public static T Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        Init();
    }

    private void OnDestroy()
    {
        Release();
    }

    protected virtual void Init()
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<T>();
            if (instance == null)
            {
                // still null, create new one
                GameObject obj = new GameObject(typeof(T).Name);
                instance = obj.AddComponent<T>();
            }

            if (isDontDestroyOnLoad)
            {
                DontDestroyOnLoad(instance.gameObject);
            }
        }
    }

    protected virtual void Release()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
