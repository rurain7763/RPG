using Gpm.Ui;
using System;
using TMPro;
using UnityEngine;

public class QuestUI : PopupUI
{
    enum TabCategory
    {
        AvailableQuests,
        InProgressQuests,
        CompletedQuests,
    }

    [SerializeField, Reference()] private TabController tabController;

    [SerializeField, Reference()] private InfiniteScroll availableQuestScroll;
    [SerializeField, Reference()] private TMP_Text descriptionText;

    [SerializeField, Reference()] private InfiniteScroll inProgressQuestScroll;
    [SerializeField, Reference()] private TMP_Text inProgressQuestProgressText;

    [SerializeField, Reference()] private InfiniteScroll completedQuestScroll;

    private QuestSystem questSystem;

    private TabCategory currentTab = TabCategory.AvailableQuests;
    private QuestData currentSelectedQuestData;
    private Quest currentSelectedQuest;

    private void Awake()
    {
        tabController.onSelected.AddListener(tab =>
        {
            if (questSystem == null)
            {
                return;
            }

            int index = tabController.GetTabIndex(tab);
            currentTab = (TabCategory)index;
            currentSelectedQuestData = null;
            Setup(questSystem);
        });
    }

    public override void OnClose(Transform parent, Action onCompleteClose = null)
    {
        base.OnClose(parent, onCompleteClose);

        currentSelectedQuestData = null;

        if (currentSelectedQuest != null)
        {
            currentSelectedQuest.CurrentStep.OnStepChanged -= OnInProgressQuestStepChanged;
            currentSelectedQuest = null;
        }
    }
    
    public void Setup(QuestSystem questSystem)
    {
        this.questSystem = questSystem;

        if (currentTab == TabCategory.AvailableQuests)
        {
            UpateAvailableQuestScroll();
        }
        else if (currentTab == TabCategory.InProgressQuests)
        {
            UpdateInProgressQuestScroll();
        }
        else if (currentTab == TabCategory.CompletedQuests)
        {
            UpdateCompletedQuestScroll();
        }
    }

    private void UpateAvailableQuestScroll()
    {
        availableQuestScroll.ClearData();

        RPG.AppDataSys.AppData.EachQuestDatas(questData =>
        {
            if (!questData.IsSatisfiedPrerequisites(questSystem))
            {
                return;
            }

            if (questData.Policy == QuestPolicy.Unique && questSystem.HasCompletedHistory(questData))
            {
                return;
            }

            if (questSystem.HasActiveQuest(questData))
            {
                return;
            }

            var scrollData = new QuestScrollItemData
            {
                QuestData = questData,
                OnClick = HandleOnClickAvailableQuestScrollItem
            };

            availableQuestScroll.InsertData(scrollData);
        });
    }

    private void UpdateInProgressQuestScroll()
    {
        inProgressQuestScroll.ClearData();

        questSystem.EachActiveQuests(quest =>
        {
            var scrollData = new QuestScrollItemData
            {
                QuestData = quest.Data,
                OnClick = HandleOnClickInProgressQuestScrollItem
            };

            inProgressQuestScroll.InsertData(scrollData);

            return true;
        });
    }

    private void UpdateCompletedQuestScroll()
    {
        completedQuestScroll.ClearData();

        RPG.AppDataSys.AppData.EachQuestDatas(questData =>
        {
            if (!questSystem.HasCompletedHistory(questData))
            {
                return;
            }

            var scrollData = new QuestScrollItemData
            {
                QuestData = questData,
                OnClick = HandleOnClickCompletedQuestScrollItem
            };

            completedQuestScroll.InsertData(scrollData);
        });
    }

    private void HandleOnClickAvailableQuestScrollItem(QuestScrollItem questItem)
    {
        descriptionText.text = questItem.QuestData.Description;
    }

    private void HandleOnClickInProgressQuestScrollItem(QuestScrollItem questItem)
    {
        if (currentSelectedQuest != null)
        {
            currentSelectedQuest.CurrentStep.OnStepChanged -= OnInProgressQuestStepChanged;
        }

        currentSelectedQuestData = questItem.QuestData;
        currentSelectedQuest = questSystem.GetActiveQuest(currentSelectedQuestData);
        OnInProgressQuestStepChanged();
        currentSelectedQuest.CurrentStep.OnStepChanged += OnInProgressQuestStepChanged;
    }

    private void OnInProgressQuestStepChanged()
    {
        inProgressQuestProgressText.text = currentSelectedQuest.CurrentStep.GetProgressText("- {0} : {1}/{2}");
    }

    private void HandleOnClickCompletedQuestScrollItem(QuestScrollItem questItem)
    {
    }
}