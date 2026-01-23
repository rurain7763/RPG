using System;
using System.Collections.Generic;

public class QuestSystem
{
    private Dictionary<UUID, Quest> activeQuests = new();
    private HashSet<UUID> completedQuestIDs = new();

    public QuestSystem()
    {
    }

    public QuestSystem(QuestSystemDTO dto, Func<UUID, QuestData> questDataResolver)
    {
        foreach (var questDTO in dto.activeQuestDTOs)
        {
            var questData = questDataResolver(questDTO.id);
            var quest = new Quest(questData, this, questDTO);
            activeQuests.Add(questDTO.id, quest);
        }

        foreach (var id in dto.completedQuestIDs)
        {
            completedQuestIDs.Add(id);
        }
    }

    public void Begin()
    {
        foreach (var pair in activeQuests)
        {
            pair.Value.Resume();
        }
    }

    public void End()
    {
        foreach (var pair in activeQuests)
        {
            pair.Value.Pause();
        }
    }

    public Quest GetActiveQuest(QuestData questData)
    {
        if (activeQuests.TryGetValue(questData.ID, out var quest))
        {
            return quest;
        }
        return null;
    }

    public bool HasActiveQuest(QuestData questData)
    {
        return activeQuests.ContainsKey(questData.ID);
    }

    public bool HasCompletedHistory(QuestData questData)
    {
        return completedQuestIDs.Contains(questData.ID);
    }

    public Quest TryBeginQuest(QuestData questData, UUID publisher)
    {
        if (activeQuests.ContainsKey(questData.ID))
        {
            Logger.Warn($"Quest {questData.DisplayName} is already in progress or completed.");
            return null;
        }

        if (questData.Policy == QuestPolicy.Unique && completedQuestIDs.Contains(questData.ID))
        {
            Logger.Warn($"Quest {questData.DisplayName} has already been completed and is unique.");
            return null;
        }

        var quest = new Quest(questData, this, publisher);
        quest.Begin();

        activeQuests[questData.ID] = quest;

        return quest;
    }

    public bool TryPauseQuest(Quest quest)
    {
        if (quest.QuestSystem != this)
        {
            Logger.Warn($"Quest {quest.Data.DisplayName} does not belong to this QuestSystem.");
            return false;
        }

        if (!quest.IsInProgress())
        {
            Logger.Warn($"Quest {quest.Data.DisplayName} is not in progress and cannot be paused.");
            return false;
        }

        quest.Pause();

        return true;
    }

    public bool TryResumeQuest(Quest quest)
    {
        if (quest.QuestSystem != this)
        {
            Logger.Warn($"Quest {quest.Data.DisplayName} does not belong to this QuestSystem.");
            return false;
        }

        if (!quest.IsInProgress())
        {
            Logger.Warn($"Quest {quest.Data.DisplayName} is not in progress and cannot be resumed.");
            return false;
        }

        quest.Resume();

        return true;
    }

    public bool TryAdvanceQuest(Quest quest)
    {
        if (quest.QuestSystem != this)
        {
            Logger.Warn($"Quest {quest.Data.DisplayName} does not belong to this QuestSystem.");
            return false;
        }

        if (!quest.CanAdvance())
        {
            Logger.Warn($"Quest {quest.Data.DisplayName} cannot be advanced at this time.");
            return false;
        }

        quest.Advance();

        return true;
    }

    public bool TryCompleteQuest(Quest quest)
    {
        if (quest.QuestSystem != this)
        {
            Logger.Warn($"Quest {quest.Data.DisplayName} does not belong to this QuestSystem.");
            return false;
        }

        if (!quest.CanBeCompleted())
        {
            Logger.Warn($"Quest {quest.Data.DisplayName} cannot be completed at this time.");
            return false;
        }
        
        quest.Complete();

        activeQuests.Remove(quest.Data.ID);
        completedQuestIDs.Add(quest.Data.ID);

        return true;
    }

    public void EachActiveQuests(Func<Quest, bool> action)
    {
        foreach (var pair in activeQuests)
        {
            if (!action(pair.Value))
            {
                return;
            }
        }
    }

    public QuestSystemDTO CaptureDTO()
    {
        var dto = new QuestSystemDTO();
        dto.activeQuestDTOs = new();
        foreach (var pair in activeQuests)
        {
            dto.activeQuestDTOs.Add(pair.Value.CaptureDTO());
        }

        dto.completedQuestIDs = new();
        foreach (var id in completedQuestIDs)
        {
            dto.completedQuestIDs.Add(id);
        }

        return dto;
    }
}

[Serializable]
public class QuestSystemDTO
{
    public List<QuestDTO> activeQuestDTOs = new();
    public List<UUID> completedQuestIDs = new();
}