using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillDisplayer : MonoBehaviour
{
    [SerializeField] private SkillCoreData skillData;
    [SerializeField] private GameObject defaultGo;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;

    public void Setup(SkillCoreData skillData)
    {
        this.skillData = skillData;

        if (defaultGo != null)
        {
            defaultGo.SetActive(false);
        }

        if (iconImage != null)
        {
            iconImage.sprite = skillData.Icon;
            iconImage.enabled = true;
        }

        if (nameText != null)
        {
            nameText.text = skillData.DisplayName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = skillData.Description;
        }
    }

    public void Cleanup()
    {
        skillData = null;

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

        if (descriptionText != null)
        {
            descriptionText.text = string.Empty;
        }
    }

    private void OnValidate()
    {
        if (skillData == null)
        {
            return;
        }

        Setup(skillData);
    }
}