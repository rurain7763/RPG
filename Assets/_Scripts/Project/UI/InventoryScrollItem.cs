using Gpm.Ui;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryScrollData : InfiniteScrollData
{
    public InventorySlot InventorySlot;
    public Action<InventoryScrollItem> OnClick;
    public Action<InventoryScrollItem> OnPointerEnter;
    public Action<InventoryScrollItem> OnPointerExit;
    public Func<InventoryScrollItem, GameObject, IDragAndDropPayload> OnPointerBeginDrag;
    public Action<InventoryScrollItem, bool> OnPointerEndDrag;

    public bool IsEmpty()
    {
        return InventorySlot == null;
    }
}

public class InventoryScrollItem : InfiniteScrollItem, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Reference("Image_Icon")] private Image iconImage;
    [SerializeField, Reference("Text_Count")] private TMP_Text countText;
    [SerializeField, Reference("")] private DragAndDropSource dragAndDropSource;

    public InventoryScrollData InventoryScrollData { get; private set; }

    private void Awake()
    {
        dragAndDropSource.OnPointerBeginDrag += OnPointerBeginDrag;
        dragAndDropSource.OnPointerEndDrag += OnPointerEndDrag;
    }

    public override void UpdateData(InfiniteScrollData scrollData)
    {
        InventoryScrollData = scrollData as InventoryScrollData;

        if (!InventoryScrollData.IsEmpty())
        {
            int itemCount = InventoryScrollData.InventorySlot.ItemCount;

            iconImage.sprite = InventoryScrollData.InventorySlot.ItemData.Icon;
            iconImage.enabled = true;

            if (itemCount == 1)
            {
                countText.enabled = false;
            }
            else
            {
                countText.text = itemCount.ToString();
                countText.enabled = true;
            }

            dragAndDropSource.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
            countText.enabled = false;
            dragAndDropSource.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryScrollData.OnClick?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InventoryScrollData == null || InventoryScrollData.IsEmpty())
        {
            return;
        }

        InventoryScrollData.OnPointerEnter?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryScrollData == null || InventoryScrollData.IsEmpty())
        {
            return;
        }

        InventoryScrollData.OnPointerExit?.Invoke(this);
    }

    private void OnPointerBeginDrag(PointerEventData eventData, GameObject proxy)
    {
        if (InventoryScrollData == null || InventoryScrollData.IsEmpty())
        {
            return;
        }

        if (InventoryScrollData.OnPointerBeginDrag == null)
        {
            return;
        }

        var payload = InventoryScrollData.OnPointerBeginDrag.Invoke(this, proxy);
        dragAndDropSource.SetPayload(payload);
    }

    private void OnPointerEndDrag(bool handled)
    {
        if (InventoryScrollData == null || InventoryScrollData.IsEmpty())
        {
            return;
        }

        if (InventoryScrollData.OnPointerEndDrag == null)
        {
            return;
        }

        InventoryScrollData.OnPointerEndDrag?.Invoke(this, handled);
    }
}