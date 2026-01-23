using Gpm.Ui;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftItemScrollItemData : InfiniteScrollData
{
    public ItemCraftingDataEntry craftingDataEntry;
    public Action<CraftItemScrollItem> OnClick;
}

public class CraftItemScrollItem : InfiniteScrollItem, IPointerDownHandler
{
    [SerializeField, Reference("Image_Icon")] private Image iconImage;
    [SerializeField, Reference("Text_Name")] private TMP_Text itemNameText;

    private CraftItemScrollItemData scrollItemData;

    public ItemCraftingDataEntry ItemCraftingDataEntry => scrollItemData.craftingDataEntry;

    public override void UpdateData(InfiniteScrollData scrollData)
    {
        scrollItemData = scrollData as CraftItemScrollItemData;

        iconImage.sprite = ItemCraftingDataEntry.ResultItemData.Icon;
        itemNameText.text = ItemCraftingDataEntry.ResultItemData.DisplayName;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        scrollItemData.OnClick?.Invoke(this);
    }
}
