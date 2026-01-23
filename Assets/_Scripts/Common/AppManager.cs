using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AppSystem : MonoBehaviour
{
    public virtual void OnAttach(AppManager appManager) { }
    public virtual void OnDetach() { }
    public virtual Type[] GetDependencySystemTypes() { return Array.Empty<Type>(); }
}

public class AppManager : SingletonBehaviour<AppManager>
{
    private Dictionary<Type, AppSystem> systems = new();

    protected override void Init()
    {
        base.Init();

        // NOTE: Attach all existing AppSystem components on this GameObject
        foreach (var system in GetComponentsInChildren<AppSystem>())
        {
            AttachSystemImpl(system.GetType(), new HashSet<Type>());
        }
    }

    public T AttachSystem<T>() where T : AppSystem
    {
        return AttachSystem(typeof(T)) as T;
    }

    public AppSystem AttachSystem(Type type)
    {
        if (!type.IsSubclassOf(typeof(AppSystem)))
        {
            throw new ArgumentException($"Type {type} is not a subclass of AppSystem");
        }

        return AttachSystemImpl(type, new HashSet<Type>());
    }

    private AppSystem AttachSystemImpl(Type targetType, HashSet<Type> visitedType)
    {
        if (visitedType.Contains(targetType))
        {
            throw new InvalidOperationException($"Cyclic dependency detected when attaching system of type {targetType}");
        }

        visitedType.Add(targetType);
        if (systems.TryGetValue(targetType, out var existingSystem))
        {
            return existingSystem;
        }

        AppSystem system = GetComponentInChildren(targetType) as AppSystem;
        if (system == null)
        {
            system = gameObject.AddComponent(targetType) as AppSystem;
        }
        systems[targetType] = system;

        foreach (var depSysType in system.GetDependencySystemTypes())
        {
            if (!systems.ContainsKey(depSysType))
            {
                AttachSystemImpl(depSysType, visitedType);
            }
        }

        system.OnAttach(this);

        return system;
    }

    public void DetachSystem<T>() where T : AppSystem
    {
        Type type = typeof(T);
        if (systems.TryGetValue(type, out var system))
        {
            system.OnDetach();
            Destroy(system);
            systems.Remove(type);
        }
    }

    public T GetSystem<T>() where T : AppSystem
    {
        Type type = typeof(T);
        if (systems.ContainsKey(type))
        {
            return systems[type] as T;
        }
        return null;
    }

    public bool HasSystem<T>() where T : AppSystem
    {
        Type type = typeof(T);
        return systems.ContainsKey(type);
    }
}
