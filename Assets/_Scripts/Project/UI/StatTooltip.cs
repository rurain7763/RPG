using TMPro;
using UnityEngine;

public class StatTooltip : ToolTip
{
    [SerializeField, Reference("Text_Name")] private TMP_Text statNameText;
    [SerializeField, Reference("Text_Description")] private TMP_Text statDescriptionText;

    public void Setup(StatData statData)
    {
        statNameText.text = statData.DisplayName;
        statDescriptionText.text = statData.Description;
    }
}