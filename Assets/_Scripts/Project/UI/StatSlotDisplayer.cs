using TMPro;
using UnityEngine;

public class StatSlotDisplayer : MonoBehaviour
{
    [SerializeField] private StatData statData;
    [SerializeField, Reference("Text_Name")] private TMP_Text statNameText;
    [SerializeField, Reference("Text_Value")] private TMP_Text statValueText;
    
    private PointerListener pointerListener;

    private IStat stat;
    private StatTooltip tooltip;

    private void Awake()
    {
        pointerListener = GetComponentInChildren<PointerListener>();

        pointerListener.OnPointerEnterEvent.AddListener((eventData) =>
        {
            if (tooltip == null)
            {
                return;
            }

            tooltip.Setup(statData);
            tooltip.ShowOnPointer(eventData.position);
        });

        pointerListener.OnPointerMoveEvent.AddListener((evenData) =>
        {
            if (tooltip == null)
            {
                return;
            }

            tooltip.ShowOnPointer(evenData.position);
        });

        pointerListener.OnPointerExitEvent.AddListener((evenData) =>
        {
            if (tooltip == null)
            {
                return;
            }

            tooltip.Hide();
        });
    }

    private void OnDestroy()
    {
        if (stat != null)
        {
            stat.OnStatChanged -= UpdateValueString;
        }
    }

    public void Setup(EntityStatSystem statSystem, StatTooltip tooltip)
    {
        this.tooltip = tooltip;
        stat = statSystem.GetStat<IStat>(statData);
        if (stat == null)
        {
            Debug.LogError($"StatSlotDisplayer.Setup: StatSystem does not have stat data {statData.name}.");
            return;
        }

        stat.OnStatChanged += UpdateValueString;

        statNameText.text = stat.DisplayName;

        UpdateValueString();
    }

    private void UpdateValueString()
    {
        statValueText.text = stat.IsPercent ? $"{stat.FinalValue * 100.0f:0.##}%" : stat.FinalValue.ToString("0.##");
    }

    private void OnValidate()
    {
        if (statData == null)
        {
            return;
        }

        if (statNameText != null)
        {
            statNameText.text = statData.DisplayName;
        }

        if (statValueText != null)
        {
            statValueText.text = statData.IsPercent ? "0%" : "0";
        }
    }
}