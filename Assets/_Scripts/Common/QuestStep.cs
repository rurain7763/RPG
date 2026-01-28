using System;
using UnityEngine;

[Serializable]
public abstract class QuestStepData
{
    public string Description;
    [SerializeReference, SubclassSelector] public IReward[] CompletionRewards;

    public abstract QuestStep CreateStep();
}

public abstract class QuestStep
{
    private QuestStepData data;

    internal Quest quest;

    public QuestStepData Data => data;
    public Quest Quest => quest;

    public event Action OnStepChanged;

    public QuestStep(QuestStepData data)
    {
        this.data = data;
    }

    protected void NotifyStepChanged()
    {
        OnStepChanged?.Invoke();
    }

    public abstract void Begin();
    public abstract void End();

    public virtual void Commit() 
    {
        foreach (var reward in Data.CompletionRewards)
        {
            reward.Grant();
        }
    }

    public abstract bool IsAchieved();
    public abstract float GetProgress();
    public abstract string GetProgressText(string format);
    public virtual string Capture() => string.Empty;
    public virtual void Restore(string data) { }

    public virtual UUID GetObjectiveTarget() => quest.Publisher;
}