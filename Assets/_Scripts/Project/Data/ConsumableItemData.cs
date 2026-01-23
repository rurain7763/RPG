using UnityEngine;

[CreateAssetMenu(menuName = "Project/Consumable Item Data", fileName = "New Consumable Item Data")]
public class ConsumableItemData : ItemData
{
    [SerializeReference, SubclassSelector] public ItemEffectData[] Effects;

    public override int Category => (int)ItemCategory.Consumable;

    public override Item CreateItem()
    {
        return new ConsumableItem(this);
    }

    public override Item CreateItem(SerialNumber serialNumber)
    {
        return new ConsumableItem(this, serialNumber);
    }
}

