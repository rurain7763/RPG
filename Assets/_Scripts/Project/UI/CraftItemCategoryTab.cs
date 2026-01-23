using Gpm.Ui;
using System;
using UnityEngine;

public class CraftItemCategoryTabData : ITabData
{
    public ItemCraftingData ItemCraftingData;
    public Action<CraftItemScrollItem> OnClick;
}

public class CraftItemCategoryTab : Gpm.Ui.TabButton
{
    [SerializeField] private CraftItemCategory category;

    public CraftItemCategory Category => category;
}