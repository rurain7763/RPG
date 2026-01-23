using Gpm.Ui;
using TMPro;
using UnityEngine;

public class CraftRequirementScrollItemData : InfiniteScrollData
{
    public ItemData ItemData;
    public int RequiredAmount;
    public int OwnedAmount;
}

public class CraftRequirementScrollItem : InfiniteScrollItem
{
    [SerializeField] private ItemDisplayer itemDisplayer;
    [SerializeField] private TMP_Text amountText;

    public override void UpdateData(InfiniteScrollData scrollData)
    {
        var data = scrollData as CraftRequirementScrollItemData;

        itemDisplayer.Setup(data.ItemData);
        amountText.text = $"{data.OwnedAmount} / {data.RequiredAmount}";
    }
}
