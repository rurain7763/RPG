using System;

[Serializable]
public class ItemReward : IReward
{
    public ItemData itemData;
    public int quantity;

    public void Grant()
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