using System.Collections.Generic;

public interface IEventRegistry
{
    void UnRegister(object listener);
    void PollEvents();
    bool HasListeners();
}

public class EventResitry<T> : IEventRegistry
{
    private readonly Queue<T> eventQueue = new();
    private readonly Queue<(object listener, T evn)> eventQueueToListener = new();
    private readonly Dictionary<object, System.Action<T>> listeners = new();

    public void Register(object listener, System.Action<T> callback)
    {
        if (!listeners.ContainsKey(listener))
        {
            listeners.Add(listener, callback);
        }
    }

    public void UnRegister(object listener)
    {
        if (listeners.ContainsKey(listener))
        {
            listeners.Remove(listener);
        }
    }

    public void DispatchEvent(T evn)
    {
        eventQueue.Enqueue(evn);
    }

    public void DispatchEventToListener(object listener, T evn)
    {
        eventQueueToListener.Enqueue((listener, evn));
    }

    public void PollEvents()
    {
        while (eventQueue.Count > 0)
        {
            var evn = eventQueue.Dequeue();
            foreach (var listener in listeners.Values)
            {
                listener.Invoke(evn);
            }
        }

        while (eventQueueToListener.Count > 0)
        {
            var (listenId, evn) = eventQueueToListener.Dequeue();
            if (listeners.TryGetValue(listenId, out var listener))
            {
                listener.Invoke(evn);
            }
        }
    }

    public bool HasListeners() => listeners.Count > 0;
}

public class EventDispatcher
{
    private readonly Dictionary<System.Type, IEventRegistry> eventRegistries = new();

    public void Register<T>(object listener, System.Action<T> callback)
    {
        var type = typeof(T);

        if (!eventRegistries.TryGetValue(type, out var registry))
        {
            registry = new EventResitry<T>();
            eventRegistries.Add(type, registry);
        }

        (registry as EventResitry<T>).Register(listener, callback);
    }

    public void Unregister<T>(object listener)
    {
        var type = typeof(T);
        if (eventRegistries.TryGetValue(type, out var registry))
        {
            registry.UnRegister(listener);
            if (!registry.HasListeners())
            {
                eventRegistries.Remove(type);
            }
        }
    }

    public void UnregisterAll(object listener)
    {
        var toRemove = new List<System.Type>();
        foreach (var (type, registry) in eventRegistries)
        {
            registry.UnRegister(listener);
            if (!registry.HasListeners())
            {
                toRemove.Add(type);
            }
        }

        foreach (var type in toRemove)
        {
            eventRegistries.Remove(type);
        }
    }

    public void DispatchEvent<T>(T evn)
    {
        var type = typeof(T);
        if (eventRegistries.TryGetValue(type, out var registry))
        {
            (registry as EventResitry<T>).DispatchEvent(evn);
        }
    }

    public void DispatchEventToListener<T>(ulong listenId, T evn)
    {
        var type = typeof(T);
        if (eventRegistries.TryGetValue(type, out var registry))
        {
            (registry as EventResitry<T>).DispatchEventToListener(listenId, evn);
        }
    }

    public void PollEvents()
    {
        foreach (var registry in eventRegistries.Values)
        {
            registry.PollEvents();
        }
    }
}