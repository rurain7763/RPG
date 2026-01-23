using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ObjectPool<T> : MonoBehaviour where T : MonoBehaviour
{
    public string PoolName = "ObjectPool";
    [SerializeField] private Transform parentTransform;
    public T Prefab;
    public int InitialSize = 10;
    public bool DynamicGrowth = true;

    readonly HashSet<T> objects = new();
    readonly Queue<T> pool = new();

    public Transform ParentTransform
    {
        get => parentTransform;
        set
        {
            if (parentTransform == value)
            {
                return;
            }

            foreach (var obj in objects)
            {
                if (obj != null && obj.gameObject != null)
                {
                    obj.transform.SetParent(value, true);
                }
            }

            parentTransform = value;
        }
    }

    public bool IsInitialized { get; private set; } = false;

    public void Initialize()
    {
        if (ParentTransform == null)
        {
            ParentTransform = transform;
        }

        IsInitialized = true;

        for (int i = 0; i < InitialSize; i++)
        {
            CreateObject();
        }
    }

    public void Cleanup()
    {
        foreach (var obj in objects)
        {
            if (obj != null && obj.gameObject != null)
            {
                OnDestroyObject(obj);
                Destroy(obj.gameObject);
            }
        }

        objects.Clear();
        pool.Clear();
    }

    void CreateObject()
    {
        T obj = Instantiate(Prefab, ParentTransform);
        obj.gameObject.SetActive(false);

        objects.Add(obj);
        pool.Enqueue(obj);

        OnCreateObject(obj);
    }

    public T GetObject()
    {
        if (pool.Count == 0)
        {
            if (DynamicGrowth)
            {
                CreateObject();
            }
            else
            {
                Logger.Warn($"[{PoolName}] Pool is empty and dynamic growth is disabled.");
                return null;
            }
        }

        T obj = pool.Dequeue();
        obj.gameObject.SetActive(true);
        OnGetObject(obj);

        return obj;
    }

    public void ReleaseObject(T obj)
    {
        if (!objects.Contains(obj))
        {
            Logger.Warn($"[{PoolName}] Trying to release an object that is not managed by this pool.");
            return;
        }

        OnReleaseObject(obj);
        obj.gameObject.SetActive(false);

        pool.Enqueue(obj);
    }

    public void DestroyObject(T obj)
    {
        if (!objects.Contains(obj))
        {
            Logger.Warn($"[{PoolName}] Trying to destroy an object that is not managed by this pool.");
            return;
        }

        OnDestroyObject(obj);
        objects.Remove(obj);

        Destroy(obj.gameObject);
    }

    public void ResetToInitialSize(bool force = false)
    {
        while (objects.Count > InitialSize)
        {
            T obj = null;
            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else
            {
                foreach (var o in objects)
                {
                    if (o.gameObject.activeSelf == false)
                    {
                        obj = o;
                        break;
                    }
                }

                if (obj == null && force)
                {
                    obj = objects.Last();
                }
            }
            if (obj != null)
            {
                DestroyObject(obj);
            }
            else
            {
                Logger.Warn($"[{PoolName}] Cannot reduce pool size further; all objects are in use.");
                break;
            }
        }
    }

    public void ReleaseAllObjects()
    {
        pool.Clear();

        foreach (var obj in objects)
        {
            OnReleaseObject(obj);
            obj.gameObject.SetActive(false);

            pool.Enqueue(obj);
        }
    }

    virtual protected void OnCreateObject(T obj) { }
    virtual protected void OnGetObject(T obj) { }
    virtual protected void OnReleaseObject(T obj) { }
    virtual protected void OnDestroyObject(T obj) { }
}