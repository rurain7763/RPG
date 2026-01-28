using Gpm.Ui;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class QuestScrollItemData : InfiniteScrollData
{
    public QuestData QuestData;
    public Action<QuestScrollItem> OnClick;
}

public class QuestScrollItem : InfiniteScrollItem, IPointerDownHandler
{
    [SerializeField, Reference("Text_Title")] private LocalizationText titleText;

    private QuestScrollItemData data;

    public QuestData QuestData => data.QuestData;

    public override void UpdateData(InfiniteScrollData scrollData)
    {
        data = scrollData as QuestScrollItemData;
        titleText.SetText($"{{{QuestData.DisplayName}}}");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        data.OnClick?.Invoke(this);
    }
}
