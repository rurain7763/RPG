public abstract class Item
{
    protected ItemData itemData;
    protected SerialNumber serialNumber;

    public ItemData ItemData => itemData;
    public SerialNumber SerialNumber => serialNumber;

    public Item(ItemData itemData)
    {
        this.itemData = itemData;
        serialNumber = SerialNumber.CreateSimple();
    }

    public Item(ItemData itemData, SerialNumber serialNumber)
    {
        this.itemData = itemData;
        this.serialNumber = serialNumber;
    }
}

