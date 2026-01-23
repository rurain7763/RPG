using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class EquipSlotDisplayer : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private EquipmentCategory equipmentCategory;
    [SerializeField, Reference("")] private ItemDisplayer itemDisplayer;

    public EquipmentItem EquippedItem { get; private set; }
    private Action onClick;

    public EquipmentCategory EquipmentCategory => equipmentCategory;

    public void Setup(EquipmentItem equipmentItem, Action onClick)
    {
        EquippedItem = equipmentItem;
        this.onClick = onClick;

        itemDisplayer.Setup(equipmentItem);
    }

    public void Clear()
    {
        EquippedItem = null;
        onClick = null;
        itemDisplayer.Cleanup();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        onClick?.Invoke();
    }
}
