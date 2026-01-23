using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class SkillSequence
{
    public virtual void Use(GameObject user, Arguments parameters = null) { }
    public virtual void UpdateParameters(Arguments parameters) { }
    public virtual void Begin() { }
    public virtual void Tick(float delta) { }
    public virtual void End() { }
}

[Serializable]
public abstract class Skill
{
    private Dictionary<string, Action<IEventData>> eventHandlers = new();
    private List<SkillSequence> sequences = new();

    private float cooldownTimer;
    private int currentSequenceIndex = -1;

    public SkillCoreData Data { get; private set; }
    public virtual float Cooldown => Data.BaseCooldown;
    public bool IsOnCooldown => cooldownTimer > 0;
    public float CooldownRemaining => cooldownTimer;
    public float CooldownFraction => Cooldown > 0 ? cooldownTimer / Cooldown : 0;

    public event Action OnCooldownChanged;

    public Skill(SkillCoreData data)
    {
        Data = data;
    }

    public void Use(GameObject user, Arguments parameters = null)
    {
        if (IsInSequence())
        {
            sequences[currentSequenceIndex].Use(user, parameters);
        }
        else
        {
            StartUse(user, parameters);
        }
    }

    protected abstract void StartUse(GameObject user, Arguments parameters = null);

    public void UpdateParameters(Arguments parameters)
    {
        if (IsInSequence())
        {
            sequences[currentSequenceIndex].UpdateParameters(parameters);
        }
    }

    public virtual void Tick(float delta)
    {
        if (IsInSequence())
        {
            sequences[currentSequenceIndex].Tick(delta);
        }
        else
        {
            ReduceCoolTime(delta);
        }
    }

    public void HandleEvents(string eventName, IEventData eventData)
    {
        if (eventHandlers.TryGetValue(eventName, out var handler))
        {
            handler.Invoke(eventData);
        }
    }

    public virtual bool IsComplete() => currentSequenceIndex == -1;
    public virtual void Cleanup() { }

    protected void RegisterEventHandler(string eventName, Action<IEventData> handler)
    {
        eventHandlers[eventName] = handler;
    }

    protected void UnregisterEventHandler(string eventName)
    {
        if (eventHandlers.ContainsKey(eventName))
        {
            eventHandlers.Remove(eventName);
        }
    }

    protected void AddSequence(SkillSequence sequence)
    {
        sequences.Add(sequence);
    }

    protected void StartSequences()
    {
        if (sequences.Count > 0)
        {
            currentSequenceIndex = 0;
            sequences[currentSequenceIndex].Begin();
        }
    }

    protected void NextSequence()
    {
        if (!IsInSequence())
        {
            return;
        }

        sequences[currentSequenceIndex].End();
        currentSequenceIndex++;
        if (currentSequenceIndex >= sequences.Count)
        {
            currentSequenceIndex = -1;
            sequences.Clear();
            return;
        }

        sequences[currentSequenceIndex].Begin();
    }

    protected void JumpToSequence(int index)
    {
        if (!IsInSequence() || index < 0 || index >= sequences.Count)
        {
            return;
        }

        sequences[currentSequenceIndex].End();
        currentSequenceIndex = index;
        sequences[currentSequenceIndex].Begin();
    }

    public void ReduceCoolTime(float amount)
    {
        cooldownTimer = Mathf.Max(0, cooldownTimer - amount);
        OnCooldownChanged?.Invoke();
    }

    public void SetToCooldown()
    {
        cooldownTimer = Cooldown;
        OnCooldownChanged?.Invoke();
    }

    public void ResetCooldown()
    {
        cooldownTimer = 0;
        OnCooldownChanged?.Invoke();
    }

    public virtual bool CanUse(GameObject user)
    {
        return !IsOnCooldown || IsInSequence();
    }

    public bool IsInSequence()
    {
        return currentSequenceIndex != -1;
    }
}