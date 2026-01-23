using System;
using System.Collections.Generic;

public class RPGAppData : AppDataTable
{
    private readonly ResourcesSystem resourcesSystem;

    private Dictionary<UUID, ItemData> itemDatas = new();
    private Dictionary<UUID, QuestData> questDatas = new();

    public RPGAppData(ResourcesSystem resourcesSystem)
    {
        this.resourcesSystem = resourcesSystem;
    }

    public override void Update()
    {
        LoadAllItemDatas();
        LoadAllQuestDatas();
    }

    private void LoadAllItemDatas()
    {
        resourcesSystem.EachAllScriptableObject<ItemData>((itemData) => itemDatas[itemData.ID] = itemData);
    }

    private void LoadAllQuestDatas()
    {
        resourcesSystem.EachAllScriptableObject<QuestData>((questData) => questDatas[questData.ID] = questData);
    }

    public ItemData GetItemData(UUID itemID)
    {
        if (itemDatas.TryGetValue(itemID, out var itemData))
        {
            return itemData;
        }
        return null;
    }

    public QuestData GetQuestData(UUID questID)
    {
        if (questDatas.TryGetValue(questID, out var questData))
        {
            return questData;
        }
        return null;
    }

    public void EachQuestDatas(Action<QuestData> action)
    {
        foreach (var questData in questDatas.Values)
        {
            action(questData);
        }
    }
}