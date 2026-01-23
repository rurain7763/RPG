using UnityEngine;

[CreateAssetMenu(menuName = "Project/Material Item Data", fileName = "New Material Item Data")]
public class MaterialItemData : ItemData
{
    public override int Category => (int)ItemCategory.Material;

    public override Item CreateItem()
    {
        return new MaterialItem(this);
    }

    public override Item CreateItem(SerialNumber serialNumber)
    {
        return new MaterialItem(this, serialNumber);
    }
}