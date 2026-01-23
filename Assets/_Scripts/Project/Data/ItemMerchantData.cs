using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemMerchanRandomDataEntry : IWeightedItem
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int weight;

    public ItemData ItemData => itemData;
    public int Weight => weight;
}

[Serializable]
public class ItemMerchantPriceDataEntry
{
    public ItemData ItemData;
    public ulong BuyPrice;
    public ulong SellPrice;
}

[CreateAssetMenu(fileName = "ItemMerchantData", menuName = "Project/Item Merchant Data")]
public class ItemMerchantData : ScriptableObject
{
    [SerializeField] private ItemMerchanRandomDataEntry[] randomDataEntries;
    [SerializeField] private ItemMerchantPriceDataEntry[] priceEntries;

    [NonSerialized] private bool isInitialized = false;

    private int totalWeight;
    private Dictionary<ItemData, ItemMerchantPriceDataEntry> priceLookup;

    private void EnsureInit()
    {
        if (isInitialized)
        {
            return;
        }

        totalWeight = 0;
        foreach (var entry in randomDataEntries)
        {
            totalWeight += entry.Weight;
        }

        priceLookup = new Dictionary<ItemData, ItemMerchantPriceDataEntry>();
        foreach (var entry in priceEntries)
        {
            priceLookup[entry.ItemData] = entry;
        }

        isInitialized = true;
    }

    public bool TryGetItemBuyPrice(ItemData itemData, out ulong price)
    {
        EnsureInit();

        price = 0;
        if (priceLookup.TryGetValue(itemData, out var entry))
        {
            price = entry.BuyPrice;
            return true;
        }
        return false;
    }

    public bool TryGetItemSellPrice(ItemData itemData, out ulong sellPrice)
    {
        EnsureInit();

        sellPrice = 0;
        if (priceLookup.TryGetValue(itemData, out var entry))
        {
            sellPrice = entry.SellPrice;
            return true;
        }
        return false;
    }

    public Item GetRandomItem()
    {
        EnsureInit();

        var rndEntry = Helper.GetRandomWeightedItem(randomDataEntries, totalWeight);
        return rndEntry.ItemData.CreateItem();
    }
}