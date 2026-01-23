using UnityEngine;

public class BlacksmithUI : PopupUI
{
    [SerializeField, Reference("Popup/Pages/Page_Storage")] private StorageDisplayer storageDisplayer;
    [SerializeField, Reference("Popup/Pages/Page_Craft")] private CraftDisplayer craftDisplayer;
    [SerializeField, Reference("Popup/ItemToolTip")] private ItemTooltip itemToolTip;

    private void OnDestroy()
    {
        storageDisplayer.Cleanup();
        craftDisplayer.Cleanup();
    }

    public void Setup(InventorySystem storageInventory, InventorySystem playerInventory)
    {
        storageDisplayer.Setup(storageInventory, playerInventory, itemToolTip);
        craftDisplayer.Setup(playerInventory, itemToolTip);
    }
}