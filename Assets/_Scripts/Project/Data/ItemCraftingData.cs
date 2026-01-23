using System;
using UnityEngine;

[Serializable]
public class ItemCraftingRequirement
{
    public ItemData ItemData;
    public int NeededAmount;
}

[Serializable]
public class ItemCraftingDataEntry
{
    public CraftItemCategory Category;
    public ItemCraftingRequirement[] Requirements;
    public ItemData ResultItemData;
    public int ResultAmount;
}

[CreateAssetMenu(fileName = "ItemCraftingData", menuName = "Project/Item Crafting Data")]
public class ItemCraftingData : ScriptableObject
{
    public ItemCraftingDataEntry[] CraftingDataEntries;
}
