using Ink.Parsed;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Entity
{
    [SerializeField] private TargetDetector2D soundDetector;
    [SerializeField] private NPCData npcData;
    [SerializeField] protected Transform dialogTextAnchor;
    [SerializeField] protected Transform questIndicatorAnchor;
    public PooledObjectPool QuestIndicatorPool { get; private set; }
    public PooledObjectPool DialogueTextPool { get; private set; }
    public QuestGiver QuestGiver { get; private set; }

    private Player playerInSurroundings;
    private PooledObject dialogueTextObject;
    private PooledObject questIndicatorObject;

    protected override void Awake()
    {
        base.Awake();

        QuestGiver = GetComponent<QuestGiver>();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (questIndicatorObject != null)
        {
            QuestIndicatorPool.ReleaseObject(questIndicatorObject);
            questIndicatorObject = null;
        }

        if (dialogueTextObject != null)
        {
            DialogueTextPool.ReleaseObject(dialogueTextObject);
            dialogueTextObject = null;
        }
    }

    public override void Begin()
    {
        base.Begin();

        QuestIndicatorPool = IncludedLevel.Environment.QuestIndicatorPool;
        DialogueTextPool = IncludedLevel.Environment.DialogueTextPool;
    }

    public override void Tick(float delta)
    {
        base.Tick(delta);

        HandleQuests();
        HandleSurroundings();
    }

    public override void End()
    {
        base.End();

        if (questIndicatorObject != null)
        {
            QuestIndicatorPool.ReleaseObject(questIndicatorObject);
            questIndicatorObject = null;
        }

        if (dialogueTextObject != null)
        {
            DialogueTextPool.ReleaseObject(dialogueTextObject);
            dialogueTextObject = null;
        }
    }

    private void HandleQuests()
    {
        if (QuestGiver == null)
        {
            return;
        }

        Player player = RPG.LocalPlayer;
        if (player == null)
        {
            return;
        }

        var interactable = GetInteractableQuest(player.QuestSystem);
        var primary = QuestGiver.GetPrimaryQuest(player.QuestSystem);
        if (interactable == null && primary == null)
        {
            if (questIndicatorObject != null)
            {
                QuestIndicatorPool.ReleaseObject(questIndicatorObject);
                questIndicatorObject = null;
            }
            return;
        }

        QuestIndicatorType indicatorType = QuestIndicatorType.QuestAvailable;
        if (interactable != null)
        {
            if (interactable.CanBeCompleted() || interactable.CanAdvance())
            {
                indicatorType = QuestIndicatorType.QuestComplete;
            }
            else if (primary != null)
            {
                indicatorType = QuestIndicatorType.QuestAvailable;
            }
            else if (interactable.IsInProgress())
            {
                indicatorType = QuestIndicatorType.QuestInProgress;
            }
        }

        QuestIndicator indicator = null;
        if (questIndicatorObject == null)
        {
            questIndicatorObject = QuestIndicatorPool.GetObject();
            indicator = questIndicatorObject.GetComponent<QuestIndicator>();
            indicator.SetAnchor(questIndicatorAnchor);
        }
        else
        {
            indicator = questIndicatorObject.GetComponent<QuestIndicator>();
        }

        indicator.SetIcon(indicatorType);
    }

    private void HandleSurroundings()
    {
        if (soundDetector == null)
        {
            return;
        }

        var first = soundDetector.DetectFirstTarget();
        if (first == null)
        {
            if (playerInSurroundings != null)
            {
                OnPlayerOutOfSurroundings(playerInSurroundings);
                playerInSurroundings = null;
            }
        }
        else
        {
            var player = first.GetComponent<Player>();
            if (player == null)
            {
                Logger.Warn("NPC detected a target that is not a Player. this should not happen.");
                return;
            }

            if (playerInSurroundings != player)
            {
                OnPlayerInSurroundings(player);
                playerInSurroundings = player;
            }
            else if (playerInSurroundings == player)
            {
                OnPlayerStayInSurroundings(player);
            }
        }
    }

    protected virtual void OnPlayerInSurroundings(Player player) { }
    protected virtual void OnPlayerStayInSurroundings(Player player) { }
    protected virtual void OnPlayerOutOfSurroundings(Player player) { }

    protected void ShowDialogueText(string text)
    {
        if (dialogueTextObject == null)
        {
            dialogueTextObject = DialogueTextPool.GetObject();
        }

        var floatingText = dialogueTextObject.GetComponent<DialogueText>();
        floatingText.SetText(text, true);
        floatingText.SetAnchor(dialogTextAnchor);
    }

    protected void HideDialogueText()
    {
        if (dialogueTextObject != null)
        {
            DialogueTextPool.ReleaseObject(dialogueTextObject);
            dialogueTextObject = null;
        }
    }

    protected void ShowRandomDialogueText()
    {
        if (npcData == null || npcData.Dialogues.Length == 0)
        {
            return;
        }

        ShowDialogueText($"{{{npcData.GetRandomDialogue()}}}");
    }

    protected bool IsQuestIndicatorActive()
    {
        return questIndicatorObject != null;
    }

    protected Quest GetInteractableQuest(QuestSystem questSystem)
    {
        if (QuestGiver == null)
        {
            return null;
        }

        Quest interactable = null;
        int bestPriority = 0;
        questSystem.EachActiveQuests(quest =>
        {
            if (quest.CurrentStep.GetObjectiveTarget() != QuestGiver.ID)
            {
                return true;
            }
            
            int priority = 0;
            if (quest.CanBeCompleted())
            {
                priority = 3;
            }
            else if (quest.CanAdvance())
            {
                priority = 2;
            }
            else if (quest.IsInProgress())
            {
                priority = 1;
            }

            if (priority > bestPriority)
            {
                bestPriority = priority;
                interactable = quest;
            }

            if (bestPriority == 3)
            {
                return false;
            }

            return true;
        });

        return interactable;
    }

    protected void OpenDialogueUI(Player player, List<(string, Action)> customActions)
    {
        var dialogue = RPG.DialogueSys.CreateDialogue("All", "NPC", npcData.Speaker);
        dialogue.GetVariable("welcome_message").SetValue(npcData.GetRandomDialogue());

        List<Quest> interactableQuests = new();
        player.QuestSystem.EachActiveQuests(quest =>
        {
            if (quest.CurrentStep.GetObjectiveTarget() == QuestGiver.ID)
            {
                interactableQuests.Add(quest);
            }

            return true;
        });

        List<QuestData> availableQuests = new();
        QuestGiver.EachAvailableQuest(player.QuestSystem, questData => availableQuests.Add(questData));

        int totalChoices = interactableQuests.Count + availableQuests.Count + customActions.Count;
        int choiceIndex = 0;
        for (; choiceIndex < totalChoices; choiceIndex++)
        {
            DialogueVariable labelVar = dialogue.GetVariable($"c{choiceIndex}_label");
            if (choiceIndex < interactableQuests.Count)
            {
                labelVar.SetValue(interactableQuests[choiceIndex].Data.DisplayName);
            }
            else if (choiceIndex - interactableQuests.Count < availableQuests.Count)
            {
                labelVar.SetValue(availableQuests[choiceIndex - interactableQuests.Count].DisplayName);
            }
            else
            {
                foreach (var (label, action) in customActions)
                {
                    if (choiceIndex - interactableQuests.Count - availableQuests.Count < customActions.Count)
                    {
                        labelVar.SetValue(label);
                    }
                }
            }
        }

        dialogue.BindExternalFunction<int>("SelectChoice", (choiceIndex) =>
        {
            if (choiceIndex < interactableQuests.Count)
            {
                var quest = interactableQuests[choiceIndex];
                if (quest.CanBeCompleted())
                {
                    dialogue.ChoosePath($"Quest_{quest.Data.ID}_Complete");
                    dialogue.Continue();
                }
                else if (quest.CanAdvance())
                {
                    dialogue.ChoosePath($"Quest_{quest.Data.ID}_{quest.CurrentStepIndex}_Complete");
                    dialogue.Continue();
                }
                else
                {
                    dialogue.ChoosePath($"Quest_{quest.Data.ID}_{quest.CurrentStepIndex}_Progress");
                    dialogue.Continue();
                }
            }
            else if (choiceIndex - interactableQuests.Count < availableQuests.Count)
            {
                var questData = availableQuests[choiceIndex - interactableQuests.Count];
                dialogue.ChoosePath($"Quest_{questData.ID}_Offer");
                dialogue.Continue();
            }
            else
            {
                customActions[choiceIndex - interactableQuests.Count - availableQuests.Count].Item2.Invoke();
            }
        });

        dialogue.BindExternalFunction("AcceptQuest", (string questId) =>
        {
            var id = new UUID(ulong.Parse(questId));

            var questData = availableQuests.Find(q => q.ID.Equals(id));
            if (questData != null)
            {
                QuestGiver.TryGiveQuest(player.QuestSystem, questData);
            }
        });

        dialogue.BindExternalFunction("AdvanceQuest", (string questId) =>
        {
            var id = new UUID(ulong.Parse(questId));
            var quest = interactableQuests.Find(q => q.Data.ID.Equals(id));
            if (quest != null)
            {
                player.QuestSystem.TryAdvanceQuest(quest);
            }
        });

        dialogue.BindExternalFunction("CompleteQuest", (string questId) =>
        {
            var id = new UUID(ulong.Parse(questId));
            var quest = interactableQuests.Find(q => q.Data.ID.Equals(id));
            if (quest != null)
            {
                player.QuestSystem.TryCompleteQuest(quest);
            }
        });

        var ui = RPG.UISys.OpenPopup<DialogueUI>();
        ui.Setup(dialogue);
    }
}