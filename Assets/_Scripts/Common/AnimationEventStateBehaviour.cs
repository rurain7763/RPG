using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventStateBehaviour : StateMachineBehaviour
{
    [Serializable]
    class EventEntry
    {
        [Range(0f, 0.999f)] public float triggerTime;
        public string eventName;
        [SerializeReference, SubclassSelector] public IEventData eventData;
    }

    [SerializeField] private bool skipPreviousEventsOnEnter = true;
    [SerializeField] private List<EventEntry> events;
    private AnimationEventReciever eventReciever;
    private int nextEventIndex;
    private float prevTime;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (eventReciever == null)
        {
            eventReciever = animator.GetComponent<AnimationEventReciever>();
        }

        if (eventReciever == null || events.Count == 0)
        {
            nextEventIndex = -1;
        }
        else
        {
            nextEventIndex = 0;
            if (skipPreviousEventsOnEnter)
            {
                float normalizedTime = stateInfo.normalizedTime % 1f;
                while (nextEventIndex < events.Count && normalizedTime >= events[nextEventIndex].triggerTime)
                {
                    nextEventIndex++;
                }
            }
        }

        prevTime = 0f;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (nextEventIndex == -1)
        {
            return;
        }

        float normalizedTime = 0f;
        if (stateInfo.loop)
        {
            normalizedTime = stateInfo.normalizedTime % 1f;

            if (prevTime > normalizedTime)
            {
                nextEventIndex = 0;
            }
        }
        else
        {
            normalizedTime = stateInfo.normalizedTime;
        }

        while (nextEventIndex < events.Count && normalizedTime >= events[nextEventIndex].triggerTime)
        {
            var entry = events[nextEventIndex];
            eventReciever.NotifyEvent(entry.eventName, entry.eventData);
            nextEventIndex++;
        }

        prevTime = normalizedTime;
    }

    private void OnValidate()
    {
        events.Sort((a, b) => a.triggerTime.CompareTo(b.triggerTime));
    }
}