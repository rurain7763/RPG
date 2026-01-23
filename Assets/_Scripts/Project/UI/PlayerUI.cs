using Gpm.Ui;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : PopupUI
{
    [SerializeField, Reference("Popup/Status/Stats/Viewport/Content")] private StatSlotDisplayer[] statSlots;
    [SerializeField, Reference("Popup/InventoryScroll")] private InfiniteScroll inventoryScroll;
    [SerializeField, Reference("Popup/Equipped/EquipSlot_Weapon")] private EquipSlotDisplayer weaponSlot;
    [SerializeField, Reference("Popup/Equipped/EquipSlot_Armor")] private EquipSlotDisplayer armorSlot;
    [SerializeField, Reference("Popup/Equipped/EquipSlot_FirstTrinket")] private EquipSlotDisplayer firstTrinketSlot;
    [SerializeField, Reference("Popup/Equipped/EquipSlot_SecondTrinket")] private EquipSlotDisplayer secondTrinketSlot;
    [SerializeField, Reference("Popup/Equipped")] private QuickItemEquipSlotDisplayer[] quickItemSlots;
    [SerializeField, Reference("Popup/ItemToolTip")] private ItemTooltip itemToolTip;
    [SerializeField, Reference("Popup/StatToolTip")] private StatTooltip statTooltip;

    private Player player;
    private EntityStatSystem statSystem;
    private EquipmentSystem equipmentSystem;
    private InventorySystem inventorySystem;

    public override void OnOpen(Transform parent)
    {
        base.OnOpen(parent);

        itemToolTip.Hide();
    }

    public override void OnClose(Transform parent, Action onCompleteClose = null)
    {
        base.OnClose(parent, onCompleteClose);

        if (equipmentSystem != null)
        {
            equipmentSystem.OnEquipmentChanged -= UpdateEquipSlots;
            equipmentSystem = null;
        }

        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryChanged -= UpdateInventoryScroll;
            inventorySystem = null;
        }
    }

    public void Setup(Player player)
    {
        this.player = player;
        statSystem = player.StatSystem;
        equipmentSystem = player.EquipmentSystem;
        inventorySystem = player.InventorySystem;

        equipmentSystem.OnEquipmentChanged += UpdateEquipSlots;
        inventorySystem.OnInventoryChanged += UpdateInventoryScroll;

        foreach (var quickItemSlot in quickItemSlots)
        {
            quickItemSlot.Setup(player.QuickItemSystem);
        }

        UpdateStats();
        UpdateEquipSlots();
        UpdateInventoryScroll();
    }

    private void UpdateStats()
    {
        foreach (var statSlot in statSlots)
        {
            statSlot.Setup(statSystem, statTooltip);
        }
    }

    private void UpdateEquipSlots()
    {
        void SetupEquipSlot(EquipSlotDisplayer slot, EquipmentItem equippedItem, Action unequipFunc)
        {
            if (equippedItem != null)
            {
                slot.Setup(equippedItem, unequipFunc);
            }
            else
            {
                slot.Clear();
            }
        }

        SetupEquipSlot(weaponSlot, equipmentSystem.EquippedWeapon, equipmentSystem.UnequipWeapon);
        SetupEquipSlot(armorSlot, equipmentSystem.EquippedArmor, equipmentSystem.UnequipArmor);
        SetupEquipSlot(firstTrinketSlot, equipmentSystem.EquippedFirstTrinket, equipmentSystem.UnequipFirstTrinket);
        SetupEquipSlot(secondTrinketSlot, equipmentSystem.EquippedSecondTrinket, equipmentSystem.UnequipSecondTrinket);
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
                OnClick = (item) =>
                {
                    var firstItem = item.InventoryScrollData.InventorySlot.GetFirstItem();
                    if (firstItem is EquipmentItem equipmentItem)
                    {
                        equipmentSystem.EquipItemByCategory(equipmentItem);
                    }
                    else if (firstItem is ConsumableItem consumableItem)
                    {
                        consumableItem.Consume(player);
                        inventorySystem.RemoveItem(consumableItem);
                    }
                },
                OnPointerEnter = (item) =>
                {
                    var firstItem = item.InventoryScrollData.InventorySlot.GetFirstItem();
                    itemToolTip.Setup(firstItem);
                    itemToolTip.Show(item.transform as RectTransform);
                },
                OnPointerExit = (item) =>
                {
                    itemToolTip.Hide();
                },
                OnPointerBeginDrag = (item, proxy) =>
                {
                    proxy.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
                    proxy.AddComponent<Image>().sprite = item.InventoryScrollData.InventorySlot.ItemData.Icon;
                    proxy.transform.SetParent(transform, true);

                    return new InventorySlotDragAndDropPayload(slot);
                },
            };
            
            inventoryScroll.InsertData(scrollData);
        }

        for (; i < inventorySystem.MaxSlotCapacity; i++)
        {
            var scrollData = new InventoryScrollData
            {
                InventorySlot = null
            };

            inventoryScroll.InsertData(scrollData);
        }
    }
}