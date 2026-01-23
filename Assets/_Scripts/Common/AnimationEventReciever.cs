using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

public class AnimationEventReciever : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<string, UnityEngine.Events.UnityEvent<IEventData>> eventsDic = new();

    public event Action<string, IEventData> OnEventTriggered;

    public void RegisterEventHandler(string eventName, UnityEngine.Events.UnityAction<IEventData> unityEvent)
    {
        if (eventsDic.TryGetValue(eventName, out var animEvent))
        {
            animEvent.AddListener(unityEvent);
        }
        else
        {
            var events = new UnityEngine.Events.UnityEvent<IEventData>();
            events.AddListener(unityEvent);
            eventsDic[eventName] = events;
        }
    }

    public void UnregisterEventHandler(string eventName, UnityEngine.Events.UnityAction<IEventData> unityEvent)
    {
        if (eventsDic.TryGetValue(eventName, out var animEvent))
        {
            animEvent.RemoveListener(unityEvent);
        }
    }

    public void NotifyEvent(string eventName, IEventData eventData)
    {
        bool handled = false;

        if (eventsDic.TryGetValue(eventName, out var animEvent))
        {
            animEvent.Invoke(eventData);
            handled = true;
        }
        
        if (OnEventTriggered != null)
        {
            OnEventTriggered.Invoke(eventName, eventData);
            handled = true;
        }
        
        if (!handled)
        {
            Logger.Warn($"AnimationEventReciever: No event handler found for event '{eventName}'");
        }
    }
}
