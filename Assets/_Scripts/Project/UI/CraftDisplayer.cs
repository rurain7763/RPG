using Gpm.Ui;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftDisplayer : MonoBehaviour
{
    [SerializeField] private ItemCraftingData itemCraftingData;
    [SerializeField, Reference("CraftListview")] private TabController craftItemCategoryTabController;
    [SerializeField, Reference("CraftPreview")] private ItemDisplayer selectedItemDisplayer;
    [SerializeField, Reference("CraftPreview/CraftRequirementsScrollView")] private InfiniteScroll selectedItemCraftRequirementsScroll;
    [SerializeField, Reference("CraftPreview/Button_Craft")] private Button craftButton;
    [SerializeField, Reference("InventoryScroll")] private InfiniteScroll inventoryScroll;

    private InventorySystem inventorySystem;
    private ItemTooltip itemTooltip;

    private ItemCraftingDataEntry selectedCraftingDataEntry;

    private void Start()
    {
        craftButton.onClick.AddListener(Craft);
    }

    public void Cleanup()
    {
        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryChanged -= UpdateCraftButtonUI;
            inventorySystem.OnInventoryChanged -= UpdateInventoryScroll;
            inventorySystem.OnInventoryChanged -= UpdateSelectedItemUI;
            inventorySystem = null;
        }
    }

    public void Setup(InventorySystem inventorySys, ItemTooltip itemTooltip)
    {
        inventorySystem = inventorySys;
        this.itemTooltip = itemTooltip;

        inventorySystem.OnInventoryChanged += UpdateInventoryScroll;
        inventorySystem.OnInventoryChanged += UpdateCraftButtonUI;
        inventorySystem.OnInventoryChanged += UpdateSelectedItemUI;

        int tabCount = craftItemCategoryTabController.GetTabCount();
        for (int i = 0; i < tabCount; i++)
        {
            var tabButton = craftItemCategoryTabController.GetTab(i);

            var tabData = new CraftItemCategoryTabData
            {
                ItemCraftingData = itemCraftingData,
                OnClick = (craftItemScrollItem) =>
                {
                    selectedCraftingDataEntry = craftItemScrollItem.ItemCraftingDataEntry;
                    UpdateSelectedItemUI();
                    UpdateCraftButtonUI();
                },
            };

            tabButton.SetData(tabData);
        }

        UpdateCraftButtonUI();
        UpdateInventoryScroll();
    }

    private bool IsAvalilableForCrafting(Item item)
    {
        if (item is EquipmentItem equipment && equipment.IsEquipped)
        {
            return false;
        }

        return true;
    }

    private void UpdateSelectedItemUI()
    {
        if (selectedCraftingDataEntry == null)
        {
            return;
        }

        selectedItemDisplayer.Setup(selectedCraftingDataEntry.ResultItemData);

        selectedItemCraftRequirementsScroll.ClearData();
        foreach (var requirement in selectedCraftingDataEntry.Requirements)
        {
            var scrollItemData = new CraftRequirementScrollItemData
            {
                ItemData = requirement.ItemData,
                RequiredAmount = requirement.NeededAmount,
                OwnedAmount = inventorySystem.GetTotalItemCount(requirement.ItemData, IsAvalilableForCrafting),
            };

            selectedItemCraftRequirementsScroll.InsertData(scrollItemData);
        }
    }

    private void UpdateCraftButtonUI()
    {
        craftButton.interactable = CanCraftSelectedItem();
    }

    private void UpdateInventoryScroll()
    {
        inventoryScroll.ClearData();

        var inventorySlots = inventorySystem.Slots;

        int i;
        for (i = 0; i < inventorySlots.Count; i++)
        {
            var slot = inventorySlots[i];

            var scrollData = new InventoryScrollData
            {
                InventorySlot = slot,
                OnClick = null,
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

            inventoryScroll.InsertData(scrollData);
        }

        for (; i < inventorySystem.MaxSlotCapacity; i++)
        {
            var scrollData = new InventoryScrollData
            {
                InventorySlot = null,
            };

            inventoryScroll.InsertData(scrollData);
        }
    }

    private bool CanCraftSelectedItem()
    {
        if (selectedCraftingDataEntry == null)
        {
            return false;
        }

        foreach (var requirement in selectedCraftingDataEntry.Requirements)
        {
            int ownedAmount = inventorySystem.GetTotalItemCount(requirement.ItemData, IsAvalilableForCrafting);
            if (ownedAmount < requirement.NeededAmount)
            {
                return false;
            }
        }

        return true;
    }

    public void Craft()
    {
        if (!CanCraftSelectedItem())
        {
            return;
        }

        using (inventorySystem.BeginTransaction())
        {
            foreach (var requirement in selectedCraftingDataEntry.Requirements)
            {
                var items = inventorySystem.GetItems(requirement.ItemData, requirement.NeededAmount, IsAvalilableForCrafting);
                inventorySystem.RemoveItems(items);
            }

            var craftedItem = selectedCraftingDataEntry.ResultItemData.CreateItem();
            inventorySystem.AddItems(new List<Item> { craftedItem });
        }
    }
}