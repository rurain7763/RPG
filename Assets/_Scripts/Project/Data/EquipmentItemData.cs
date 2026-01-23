using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Project/Equipment Item Data", fileName = "New Equipment Item Data")]
public class EquipmentItemData : ItemData
{
    public EquipmentCategory EquipmentCategory;
    public List<StatModifierData> StatModifiers;
    [SerializeReference, SubclassSelector] public ItemEffectData UniqueEffect;

    public override int Category => (int)ItemCategory.Equipment;

    public override Item CreateItem()
    {
        return new EquipmentItem(this);
    }

    public override Item CreateItem(SerialNumber serialNumber)
    {
        return new EquipmentItem(this, serialNumber);
    }
}

