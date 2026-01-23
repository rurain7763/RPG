using System;

public class EntityQuickItemSystem
{
    private Entity owner;
    private InventorySystem inventorySystem;

    private ConsumableItemData quickItem1;
    private ConsumableItemData quickItem2;

    public event Action<int, ConsumableItemData> OnQuickItemSet;
    public event Action<int> OnQuickItemCountChanged;
    public event Action<int> OnQuickItemUsed;

    public EntityQuickItemSystem(Entity owner)
    {
        this.owner = owner;
        
        if (owner is not IHasInventory hasInventory)
        {
            throw new InvalidOperationException("EntityQuickItemSystem requires an entity that has an inventory.");
        }

        inventorySystem = hasInventory.InventorySystem;
        inventorySystem.OnInventoryChanged += HandleInventoryChanged;
    }

    private void HandleInventoryChanged()
    {
        for (int slotNumber = 1; slotNumber <= 2; slotNumber++)
        {
            ConsumableItemData quickItem = GetQuickItem(slotNumber);
            if (quickItem != null)
            {
                OnQuickItemCountChanged?.Invoke(slotNumber);
            }
        }
    }

    public void SetQuickItem(int slotNumber, ConsumableItemData itemData)
    {
        switch (slotNumber)
        {
            case 1:
                quickItem1 = itemData;
                break;
            case 2:
                quickItem2 = itemData;
                break;
        }

        OnQuickItemSet?.Invoke(slotNumber, itemData);
    }

    public void ClearQuickItem(int slotNumber)
    {
        switch (slotNumber)
        {
            case 1:
                quickItem1 = null;
                break;
            case 2:
                quickItem2 = null;
                break;
        }

        OnQuickItemSet?.Invoke(slotNumber, null);
    }

    public ConsumableItemData GetQuickItem(int slotNumber)
    {
        return slotNumber switch
        {
            1 => quickItem1,
            2 => quickItem2,
            _ => null
        };
    }

    public bool TryUseQuickItem(int slotNumber)
    {
        ConsumableItemData quickItem = slotNumber switch
        {
            1 => quickItem1,
            2 => quickItem2,
            _ => null
        };

        if (quickItem == null)
        {
            return false;
        }

        var firstItem = inventorySystem.GetFirstItem(quickItem) as ConsumableItem;
        if (firstItem == null)
        {
            return false;
        }
        
        firstItem.Consume(owner);
        inventorySystem.RemoveItem(firstItem);

        OnQuickItemUsed?.Invoke(slotNumber);

        return true;
    }

    public int GetQuickItemCount(int slotNumber)
    {
        ConsumableItemData quickItem = slotNumber switch
        {
            1 => quickItem1,
            2 => quickItem2,
            _ => null
        };

        if (quickItem == null)
        {
            return 0;
        }

        return inventorySystem.GetTotalItemCount(quickItem);
    }
}