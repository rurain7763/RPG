using Gpm.Ui;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MerchantUI : PopupUI
{
    [SerializeField, Reference("Popup/Text_GoldAmount")] private TMP_Text goldAmountText;
    [SerializeField, Reference("Popup/MerchantInventoryScroll")] private InfiniteScroll storageInventoryScroll;
    [SerializeField, Reference("Popup/PlayerInventoryScroll")] private InfiniteScroll playerInventoryScroll;
    [SerializeField, Reference("Popup/ItemTooltip")] private ItemTooltip itemTooltip;

    private ItemMerchantData merchantData;
    private InventorySystem merchantInventory;
    private InventorySystem playerInventory;

    public override void OnOpen(Transform parent)
    {
        base.OnOpen(parent);

        itemTooltip.Hide();
        itemTooltip.SetActivePriceText(true);
    }

    public override void OnClose(Transform parent, Action onCompleteClose = null)
    {
        base.OnClose(parent, onCompleteClose);

        if (merchantInventory != null)
        {
            merchantInventory.OnInventoryChanged -= UpdateMerchantInventoryScroll;
            merchantInventory = null;
        }

        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= UpdatePlayerInventoryScroll;
            playerInventory = null;
        }
    }

    // This is test code to update gold amount in real-time. not efficient but works for now.
    private void Update()
    {
        var table = RPG.UserDataSys.PlayData;
        goldAmountText.text = $"{table.Gold} G";
    }

    public void Setup(ItemMerchantData merchantData, InventorySystem merchantInventory, InventorySystem playerInventory)
    {
        this.merchantData = merchantData;
        this.merchantInventory = merchantInventory;
        this.playerInventory = playerInventory;

        merchantInventory.OnInventoryChanged += UpdateMerchantInventoryScroll;
        playerInventory.OnInventoryChanged += UpdatePlayerInventoryScroll;

        UpdatePlayerInventoryScroll();
        UpdateMerchantInventoryScroll();
        itemTooltip.Hide();
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
                    bool sellAll = Input.GetKey(KeyCode.LeftControl);
                    if (sellAll)
                    {
                        SellAllItemsInSlot(item.InventoryScrollData.InventorySlot);
                    }
                    else
                    {
                        SellSingleItemInSlot(item.InventoryScrollData.InventorySlot);
                    }
                },
                OnPointerEnter = (item) =>
                {
                    var firstItem = item.InventoryScrollData.InventorySlot.GetFirstItem();
                    itemTooltip.Setup(firstItem);
                    if (merchantData.TryGetItemSellPrice(firstItem.ItemData, out ulong sellPrice))
                    {
                        itemTooltip.SetPriceText($"Sell: {sellPrice} G");
                    }
                    else
                    {
                        itemTooltip.SetPriceText("Cannot Sell");
                    }
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

    private void UpdateMerchantInventoryScroll()
    {
        storageInventoryScroll.Clear();

        var inventorySlots = merchantInventory.Slots;

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            var slot = inventorySlots[i];
            var scrollData = new InventoryScrollData
            {
                InventorySlot = slot,
                OnClick = (item) =>
                {
                    bool buyAll = Input.GetKey(KeyCode.LeftControl);
                    if (buyAll)
                    {
                        BuyAllItemsInSlot(item.InventoryScrollData.InventorySlot);
                    }
                    else
                    {
                        BuySingleItemInSlot(item.InventoryScrollData.InventorySlot);
                    }
                },
                OnPointerEnter = (item) =>
                {
                    var firstItem = item.InventoryScrollData.InventorySlot.GetFirstItem();
                    itemTooltip.Setup(firstItem);
                    if (merchantData.TryGetItemBuyPrice(firstItem.ItemData, out ulong buyPrice))
                    {
                        itemTooltip.SetPriceText($"Buy: {buyPrice} G");
                    }
                    else
                    {
                        itemTooltip.SetPriceText("Not for Sale");
                    }
                    itemTooltip.Show(item.transform as RectTransform);
                },
                OnPointerExit = (item) =>
                {
                    itemTooltip.Hide();
                },
            };

            storageInventoryScroll.InsertData(scrollData);
        }
    }

    private bool IsAvailableItemToSell(Item item, out ulong sellPrice)
    {
        sellPrice = 0;

        if (item is EquipmentItem equipment && equipment.IsEquipped)
        {
            return false;
        }

        if (!merchantData.TryGetItemSellPrice(item.ItemData, out sellPrice))
        {
            return false;
        }

        return true;
    }

    public void SellSingleItemInSlot(InventorySlot slot)
    {
        var targetItem = slot.GetFirstItem();
        if (!IsAvailableItemToSell(targetItem, out ulong sellPrice))
        {
            return;
        }

        if (!playerInventory.RemoveItem(slot, targetItem))
        {
            return;
        }

        var table = RPG.UserDataSys.PlayData;
        table.Gold += sellPrice;
    }

    public void SellAllItemsInSlot(InventorySlot slot)
    {
        ulong totalSellPrice = 0;

        List<Item> itemsToSell = new();
        foreach (var item in slot.Items)
        {
            if (IsAvailableItemToSell(item, out ulong sellPrice))
            {
                itemsToSell.Add(item);
                totalSellPrice += sellPrice;
            }
        }
        playerInventory.RemoveItems(itemsToSell);

        var table = RPG.UserDataSys.PlayData;
        table.Gold += totalSellPrice;
    }

    private bool IsAvailableItemToBuy(Item item, out ulong buyPrice)
    {
        buyPrice = 0;
        return merchantData.TryGetItemBuyPrice(item.ItemData, out buyPrice);
    }

    public void BuySingleItemInSlot(InventorySlot slot)
    {
        var targetItem = slot.GetFirstItem();
        if (!playerInventory.CanAddItem(targetItem))
        {
            return;
        }

        if (!IsAvailableItemToBuy(targetItem, out ulong buyPrice))
        {
            return;
        }

        var table = RPG.UserDataSys.PlayData;
        if (table.Gold < buyPrice)
        {
            return;
        }

        table.Gold -= buyPrice;

        playerInventory.AddItem(targetItem);
        merchantInventory.RemoveItem(slot, targetItem);
    }

    public void BuyAllItemsInSlot(InventorySlot slot)
    {
        var table = RPG.UserDataSys.PlayData;

        ulong remainingGold = table.Gold;

        List<Item> itemsToBuy = new();
        foreach (var item in slot.Items)
        {
            if (!playerInventory.CanAddItem(item))
            {
                break;
            }

            if (!IsAvailableItemToBuy(item, out ulong buyPrice))
            {
                continue;
            }

            if (remainingGold < buyPrice)
            {
                break;
            }

            itemsToBuy.Add(item);
            remainingGold -= buyPrice;
        }

        table.Gold = remainingGold;

        playerInventory.AddItems(itemsToBuy);
        merchantInventory.RemoveItems(itemsToBuy);
    }
}
