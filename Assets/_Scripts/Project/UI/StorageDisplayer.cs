using Gpm.Ui;
using System.Collections.Generic;
using UnityEngine;

public class StorageDisplayer : MonoBehaviour
{
    [SerializeField, Reference("StorageInventoryScroll")] private InfiniteScroll storageInventoryScroll;
    [SerializeField, Reference("PlayerInventoryScroll")] private InfiniteScroll playerInventoryScroll;

    private InventorySystem storageInventory;
    private InventorySystem playerInventory;
    private ItemTooltip itemTooltip;

    public void Cleanup()
    {
        if (storageInventory != null)
        {
            storageInventory.OnInventoryChanged -= UpdateStorageInventoryScroll;
            storageInventory = null;
        }

        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= UpdatePlayerInventoryScroll;
            playerInventory = null;
        }
    }

    public void Setup(InventorySystem storageInventory, InventorySystem playerInventory, ItemTooltip itemTooltip)
    {
        this.storageInventory = storageInventory;
        this.playerInventory = playerInventory;
        this.itemTooltip = itemTooltip;

        storageInventory.OnInventoryChanged += UpdateStorageInventoryScroll;
        playerInventory.OnInventoryChanged += UpdatePlayerInventoryScroll;

        UpdatePlayerInventoryScroll();
        UpdateStorageInventoryScroll();
        itemTooltip.Hide();
    }

    private void TransferSingleItemInSlot(InventorySystem from, InventorySystem to, InventorySlot targetSlot)
    {
        var targetItem = targetSlot.GetFirstItem();

        if (targetItem is EquipmentItem equipment && equipment.IsEquipped)
        {
            return;
        }

        if (!to.CanAddItem(targetItem))
        {
            return;
        }

        if (!from.RemoveItem(targetSlot, targetItem))
        {
            return;
        }

        to.AddItem(targetItem);
    }

    private void TransferAllItemsInSlot(InventorySystem from, InventorySystem to, InventorySlot targetSlot)
    {
        IEnumerable<Item> items = targetSlot.Items;
        if (targetSlot.ItemData is EquipmentItemData)
        {
            var list = new List<Item>();
            foreach (var item in targetSlot.Items)
            {
                var equipment = item as EquipmentItem;

                if (!equipment.IsEquipped)
                {
                    list.Add(item);
                }
            }

            items = list;
        }
        
        to.CanAddItems(items, out var canAddItems, out var cannotAddItems);
        from.RemoveItems(canAddItems);
        to.AddItems(canAddItems);
    }

    private void UpdatePlayerInventoryScroll()
    {
        playerInventoryScroll.Clear();

        var inventorySlots = playerInventory.Slots;

        int i;
        for (i = 0; i < inventorySlots.Count; i++)
        {
            var slot = inventorySlots[i];

            var scrollData = new InventoryScrollData
            {
                InventorySlot = slot,
                OnClick = (item) =>
                {
                    bool transferAll = Input.GetKey(KeyCode.LeftControl);
                    var inventorySlot = item.InventoryScrollData.InventorySlot;

                    if (transferAll)
                    {
                        TransferAllItemsInSlot(playerInventory, storageInventory, inventorySlot);
                    }
                    else
                    {
                        TransferSingleItemInSlot(playerInventory, storageInventory, inventorySlot);
                    }
                },
                OnPointerEnter = (item) =>
                {
                    var firstItem = item.InventoryScrollData.InventorySlot.GetFirstItem();
                    itemTooltip.Setup(firstItem);
                    itemTooltip.Show(item.transform as RectTransform);
                },
                OnPointerExit = (item) =>
                {
                    itemTooltip.Hide();
                },
            };

            playerInventoryScroll.InsertData(scrollData);
        }

        for (; i < playerInventory.MaxSlotCapacity; i++)
        {
            var scrollData = new InventoryScrollData
            {
                InventorySlot = null,
            };

            playerInventoryScroll.InsertData(scrollData);
        }
    }

    private void UpdateStorageInventoryScroll()
    {
        storageInventoryScroll.Clear();

        var inventorySlots = storageInventory.Slots;

        int i;
        for (i = 0; i < inventorySlots.Count; i++)
        {
            var slot = inventorySlots[i];
            var scrollData = new InventoryScrollData
            {
                InventorySlot = slot,
                OnClick = (item) =>
                {
                    bool transferAll = Input.GetKey(KeyCode.LeftControl);
                    var inventorySlot = item.InventoryScrollData.InventorySlot;

                    if (transferAll)
                    {
                        TransferAllItemsInSlot(storageInventory, playerInventory, inventorySlot);
                    }
                    else
                    {
                        TransferSingleItemInSlot(storageInventory, playerInventory, inventorySlot);
                    }
                },
                OnPointerEnter = (item) =>
                {
                    var firstItem = item.InventoryScrollData.InventorySlot.GetFirstItem();
                    itemTooltip.Setup(firstItem);
                    itemTooltip.Show(item.transform as RectTransform);
                },
                OnPointerExit = (item) =>
                {
                    itemTooltip.Hide();
                },
            };

            storageInventoryScroll.InsertData(scrollData);
        }

        for (; i < storageInventory.MaxSlotCapacity; i++)
        {
            var scrollData = new InventoryScrollData
            {
                InventorySlot = null,
            };

            storageInventoryScroll.InsertData(scrollData);
        }
    }
}