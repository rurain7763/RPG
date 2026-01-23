using TMPro;
using UnityEngine;

[RequireComponent(typeof(ItemDisplayer))]
public class ItemTooltip : ToolTip
{
    [SerializeField] private TMP_Text priceText;

    private ItemDisplayer itemDisplayer;

    public Item Item { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        itemDisplayer = GetComponent<ItemDisplayer>();
    }

    public void Setup(Item item)
    {
        Item = item;
        itemDisplayer.Setup(Item);
    }

    public void SetActivePriceText(bool isActive)
    {
        priceText.gameObject.SetActive(isActive);
    }

    public void SetPriceText(string text)
    {
        priceText.text = text;
    }
}