using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDisplayer : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private GameObject defaultGo;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text categoryText;
    [SerializeField] private TMP_Text equipmentCategoryText;
    [SerializeField] private TMP_Text equipmentStatsText;
    [SerializeField] private TMP_Text descriptionText;

    public void Setup(Item item)
    {
        itemData = item.ItemData;

        UpdateGeneralItemUIs();
        UpdateEquipmentItemUIs(item);
    }

    public void Setup(ItemData itemData)
    {
        this.itemData = itemData;

        UpdateGeneralItemUIs();

        if (itemData is EquipmentItemData equipmentItemData)
        {
            var tempItem = equipmentItemData.CreateItem() as EquipmentItem;
            UpdateEquipmentItemUIs(tempItem);
        }
    }

    public void Cleanup()
    {
        itemData = null;

        if (defaultGo != null)
        {
            defaultGo.SetActive(true);
        }

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (nameText != null)
        {
            nameText.text = string.Empty;
        }

        if (categoryText != null)
        {
            categoryText.text = string.Empty;
        }

        if (equipmentCategoryText != null)
        {
            equipmentCategoryText.text = string.Empty;
        }

        if (equipmentStatsText != null)
        {
            equipmentStatsText.text = string.Empty;
        }

        if (descriptionText != null)
        {
            descriptionText.text = string.Empty;
        }
    }

    private void UpdateGeneralItemUIs()
    {
        if (defaultGo != null)
        {
            defaultGo.SetActive(false);
        }

        if (iconImage != null)
        {
            iconImage.sprite = itemData.Icon;
            iconImage.enabled = true;
        }

        if (nameText != null)
        {
            nameText.text = itemData.DisplayName;
        }

        if (categoryText != null)
        {
            categoryText.text = $"{(ItemCategory)itemData.Category}";
        }

        if (descriptionText != null)
        {
            descriptionText.text = itemData.Description;
        }
    }

    private void UpdateEquipmentItemUIs(Item item)
    {
        var equipmentItem = item as EquipmentItem;
        if (equipmentItem == null)
        {
            if (equipmentCategoryText != null)
            {
                equipmentCategoryText.text = string.Empty;
            }

            if (equipmentStatsText != null)
            {
                equipmentStatsText.text = string.Empty;
            }

            return;
        }

        if (equipmentCategoryText != null)
        {
            equipmentCategoryText.text = $"{equipmentItem.ItemData.EquipmentCategory}";
        }

        if (equipmentStatsText != null)
        {
            StringBuilder statsTextBuilder = new();
            foreach (var (statData, statModifier) in equipmentItem.StatModifiers)
            {
                string signString = statModifier.Value >= 0 ? "+" : "";
                string valueString = statData.IsPercent ? $"{statModifier.Value * 100.0f}%" : $"{statModifier.Value}";
                statsTextBuilder.AppendLine($"{statData.DisplayName}: {signString}{valueString}");
            }

            equipmentStatsText.text = statsTextBuilder.ToString();
        }
    }

    private void OnValidate()
    {
        if (itemData == null)
        {
            return;
        }

        Setup(itemData);
    }
}
