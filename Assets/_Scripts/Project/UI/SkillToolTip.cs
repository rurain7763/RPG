using TMPro;
using UnityEngine;

public class SkillToolTip : ToolTip
{
    [SerializeField, Reference("Text_Name")] private TMP_Text nameText;
    [SerializeField, Reference("Text_Description")] private TMP_Text descriptionText;
    [SerializeField, Reference("Text_Requirements")] private TMP_Text requirementsText;

    public void Setup(SkillDataObject skillData)
    {
        nameText.text = skillData.DisplayName;
        descriptionText.text = skillData.Description;
        requirementsText.text = string.Empty;
    }
}