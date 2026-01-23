using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemDropDataEntry : IWeightedItem
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int weight;

    public ItemData ItemData => itemData;
    public int Weight => weight;
}

[CreateAssetMenu(fileName = "ItemDropData", menuName = "Project/Item Drop Data")]
public class ItemDropData : ScriptableObject
{
    [SerializeField] private ItemDropDataEntry[] dropItemEntries;
    [SerializeField] private int minDropCount = 1;
    [SerializeField] private int maxDropCount = 1;

    [NonSerialized] private bool isInitialized = false;

    private int totalWeight;

    private void EnsureInit()
    {
        if (isInitialized)
        {
            return;
        }

        totalWeight = 0;
        foreach (var entry in dropItemEntries)
        {
            totalWeight += entry.Weight;
        }

        isInitialized = true;
    }

    public List<Item> GetRandomDropItems()
    {
        EnsureInit();
        int dropCount = UnityEngine.Random.Range(minDropCount, maxDropCount + 1);

        List<Item> droppedItems = new List<Item>();
        for (int i = 0; i < dropCount; i++)
        {
            var rndEntry = Helper.GetRandomWeightedItem(dropItemEntries, totalWeight);
            droppedItems.Add(rndEntry.ItemData.CreateItem());
        }

        return droppedItems;
    }
}