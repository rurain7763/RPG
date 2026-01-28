using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Data", menuName = "Common/Quest Data")]
public class QuestData : ScriptableObject
{
    public UUID ID;
    public string DisplayName;
    [TextArea] public string Description;
    public QuestPolicy Policy;
    [SerializeReference, SubclassSelector] public IQuestPrerequisite[] Prerequisites;
    [SerializeReference, SubclassSelector] public QuestStepData[] Steps;
    [SerializeReference, SubclassSelector] public IReward[] StartRewards;
    [SerializeReference, SubclassSelector] public IReward[] CompletionRewards;

    private void Awake()
    {
        if (ID.IsValid())
        {
            return;
        }

        ID.Generate();
    }

    public bool IsSatisfiedPrerequisites(QuestSystem questSystem)
    {
        foreach (var prerequisite in Prerequisites)
        {
            if (!prerequisite.IsSatisfied(questSystem))
            {
                return false;
            }
        }
        return true;
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (ID.IsValid() == false)
        {
            ID.Generate();
        }
    }
}

public enum QuestPolicy
{
    Repeatable,
    Unique,
}

public interface IQuestPrerequisite
{
    bool IsSatisfied(QuestSystem questSystem);
}

[Serializable]
public class QuestQuestPrerequisites : IQuestPrerequisite
{
    public QuestData questData;

    public bool IsSatisfied(QuestSystem questSystem)
    {
        return questSystem.HasCompletedHistory(questData);
    }
}
