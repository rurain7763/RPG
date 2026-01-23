using System;

[Serializable]
public class ItemQuestReward : IQuestReward
{
    public ItemData itemData;
    public int quantity;

    public void Grant(QuestSystem questSystem)
    {
        using (RPG.UserDataSys.PlayData.Inventory.BeginTransaction())
        {
            for (int i = 0; i < quantity; i++)
            {
                Item newItem = itemData.CreateItem();
                RPG.UserDataSys.PlayData.Inventory.AddItem(newItem);
            }
        }
    }
}