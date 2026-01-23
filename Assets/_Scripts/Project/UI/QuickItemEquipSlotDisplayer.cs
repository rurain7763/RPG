using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuickItemEquipSlotDisplayer : MonoBehaviour
{
    [SerializeField] private int slotNumber;
    [SerializeField, Reference("")] private ItemDisplayer quickItemDisplayer;
    [SerializeField, Reference("Text_Count")] private TMP_Text quickItemCountText;
    [SerializeField] private DragAndDropTarget dragAndDropTarget;
    [SerializeField] private Image useFeedbackImage;

    private EntityQuickItemSystem quickItemSystem;
    private Coroutine useFeedbackCoroutine;

    private void Awake()
    {
        if (dragAndDropTarget != null)
        {
            dragAndDropTarget.OnPayloadDrop += HandlePayload;
        }
    }

    private void OnDestroy()
    {
        if (quickItemSystem != null)
        {
            quickItemSystem.OnQuickItemSet -= HandleQuickItemSet;
            quickItemSystem.OnQuickItemCountChanged -= HandleQuickItemCountChanged;
            quickItemSystem.OnQuickItemUsed -= HandleQuickItemUsed;
        }
    }

    public void Setup(EntityQuickItemSystem quickItemSystem)
    {
        if (this.quickItemSystem != null)
        {
            this.quickItemSystem.OnQuickItemSet -= HandleQuickItemSet;
            this.quickItemSystem.OnQuickItemCountChanged -= HandleQuickItemCountChanged;
            this.quickItemSystem.OnQuickItemUsed -= HandleQuickItemUsed;
        }
        this.quickItemSystem = quickItemSystem;

        quickItemSystem.OnQuickItemSet += HandleQuickItemSet;
        quickItemSystem.OnQuickItemCountChanged += HandleQuickItemCountChanged;
        quickItemSystem.OnQuickItemUsed += HandleQuickItemUsed;

        UpdateItemDisplay();
        UpdateCountText();
    }

    private void UpdateItemDisplay()
    {
        var quickItemData = quickItemSystem.GetQuickItem(slotNumber);
        if (quickItemData == null)
        {
            quickItemDisplayer.Cleanup();
            return;
        }

        quickItemDisplayer.Setup(quickItemData);
        UpdateCountText();
    }

    private void UpdateCountText()
    {
        var quickItemData = quickItemSystem.GetQuickItem(slotNumber);

        if (quickItemData == null)
        {
            quickItemCountText.text = "";
            return;
        }

        int itemCount = quickItemSystem.GetQuickItemCount(slotNumber);
        if (itemCount == 0)
        {
            quickItemCountText.text = "EMPTY";
        }
        else
        {
            quickItemCountText.text = itemCount.ToString();
        }
    }

    private void HandleQuickItemSet(int changedSlotNumber, ConsumableItemData itemData)
    {
        if (changedSlotNumber != slotNumber)
        {
            return;
        }

        UpdateItemDisplay();
        UpdateCountText();
    }

    private void HandleQuickItemCountChanged(int changedSlotNumber)
    {
        if (changedSlotNumber != slotNumber)
        {
            return;
        }

        UpdateCountText();
    }

    private void HandleQuickItemUsed(int usedSlotNumber)
    {
        if (usedSlotNumber != slotNumber)
        {
            return;
        }

        if (useFeedbackImage == null)
        {
            return;
        }

        if (useFeedbackCoroutine != null)
        {
            StopCoroutine(useFeedbackCoroutine);
        }

        useFeedbackCoroutine = StartCoroutine(HandleQuickItemUsedCo());
    }

    private IEnumerator HandleQuickItemUsedCo()
    {
        useFeedbackImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        useFeedbackImage.gameObject.SetActive(false);
    }

    private bool HandlePayload(IDragAndDropPayload payload)
    {
        if (payload is not InventorySlotDragAndDropPayload slotPayload)
        {
            return false;
        }

        if (slotPayload.InventorySlot.ItemData is not ConsumableItemData consumableItemData)
        {
            return false;
        }

        if (quickItemSystem == null)
        {
            return false;
        }

        quickItemSystem.SetQuickItem(slotNumber, consumableItemData);

        return true;
    }
}
