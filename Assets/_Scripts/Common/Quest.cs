using System;

public class Quest
{
    private readonly QuestData data;
    private readonly QuestSystem questSystem;
    private readonly UUID publisher;

    private int currentStepIndex;
    public QuestStep CurrentStep { get; private set; }
    public int CurrentStepIndex => currentStepIndex;

    public QuestData Data => data;
    public UUID Publisher => publisher;
    public QuestSystem QuestSystem => questSystem;
    public bool IsPaused { get; private set; }

    internal Quest(QuestData data, QuestSystem questSystem, UUID publisher)
    {
        this.data = data;
        this.questSystem = questSystem;
        this.publisher = publisher;
        currentStepIndex = -1;
    }

    internal Quest(QuestData data, QuestSystem questSystem, QuestDTO saveData)
    {
        this.data = data;
        this.questSystem = questSystem;
        this.publisher = saveData.publisher;
        currentStepIndex = saveData.currentStepIndex;
        if (currentStepIndex >= 0 && currentStepIndex < Data.Steps.Length)
        {
            CurrentStep = Data.Steps[currentStepIndex].CreateStep();
            CurrentStep.quest = this;
            CurrentStep.Restore(saveData.currentStepData);
            IsPaused = true;
        }
    }

    public bool IsInProgress()
    {
        return CurrentStep != null;
    }

    public bool CanAdvance()
    {
        return IsInProgress() && CurrentStep.IsAchieved() && currentStepIndex < Data.Steps.Length - 1;
    }

    public bool CanBeCompleted()
    {
        return currentStepIndex == Data.Steps.Length - 1 && CurrentStep.IsAchieved();
    }

    public QuestDTO CaptureDTO()
    {
        var dto = new QuestDTO();
        dto.id = Data.ID;
        dto.publisher = publisher;
        dto.currentStepIndex = currentStepIndex;
        if (IsInProgress())
        {
            dto.currentStepData = CurrentStep.Capture();
        }
        return dto;
    }

    internal void Begin()
    {
        foreach (var reward in Data.StartRewards)
        {
            reward.Grant();
        }

        currentStepIndex = 0;
        CurrentStep = Data.Steps[currentStepIndex].CreateStep();
        CurrentStep.quest = this;
        CurrentStep.Begin();
    }

    internal void Pause()
    {
        if (IsPaused)
        {
            return;
        }

        CurrentStep.End();
        IsPaused = true;
    }

    internal void Resume()
    {
        if (!IsPaused)
        {
            return;
        }

        CurrentStep.Begin();
        IsPaused = false;
    }

    internal void Advance()
    {
        CurrentStep.Commit();
        CurrentStep.End();

        currentStepIndex++;
        CurrentStep = Data.Steps[currentStepIndex].CreateStep();
        CurrentStep.Begin();
    }

    internal void Complete()
    {
        CurrentStep.Commit();
        CurrentStep.End();

        foreach (var reward in Data.CompletionRewards)
        {
            reward.Grant();
        }
        CurrentStep = null;
        currentStepIndex = Data.Steps.Length;
    }
}

[Serializable]
public class QuestDTO
{
    public UUID id;
    public UUID publisher;
    public int currentStepIndex;
    public string currentStepData;
}
