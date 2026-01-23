using System;
using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    [Serializable]
    struct QuestInfo
    {
        public QuestData questData;
        public int priority;
    }

    [SerializeField] private UUID id;
    [SerializeField] private QuestInfo[] questsToOffer;

    public UUID ID => id;

    private void Awake()
    {
        Array.Sort(questsToOffer, (a, b) => b.priority.CompareTo(a.priority));
    }

    private bool IsQuestAvailable(QuestSystem questSystem, QuestData questData)
    {
        if (!questData.IsSatisfiedPrerequisites(questSystem))
        {
            return false;
        }

        if (questData.Policy == QuestPolicy.Unique && questSystem.HasCompletedHistory(questData))
        {
            return false;
        }

        if (questSystem.HasActiveQuest(questData))
        {
            return false;
        }

        return true;
    }

    public QuestData GetPrimaryQuest(QuestSystem questSystem)
    {
        foreach (var questInfo in questsToOffer)
        {
            if (!IsQuestAvailable(questSystem, questInfo.questData))
            {
                continue;
            }

            return questInfo.questData;
        }

        return null;
    }

    public void EachAvailableQuest(QuestSystem questSystem, Action<QuestData> callback)
    {
        foreach (var questInfo in questsToOffer)
        {
            if (!IsQuestAvailable(questSystem, questInfo.questData))
            {
                continue;
            }

            callback?.Invoke(questInfo.questData);
        }
    }

    public Quest TryGiveQuest(QuestSystem questSystem, QuestData questData)
    {
        if (!questData.IsSatisfiedPrerequisites(questSystem))
        {
            return null;
        }

        if (questData.Policy == QuestPolicy.Unique && questSystem.HasCompletedHistory(questData))
        {
            return null;
        }

        if (questSystem.HasActiveQuest(questData))
        {
            return null;
        }

        return questSystem.TryBeginQuest(questData, id);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (id.IsValid() == false)
        {
            id.Generate();
        }
    }
}