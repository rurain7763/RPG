using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillEquipSlotDisplayer : MonoBehaviour
{
    [SerializeField] private SkillCoreData skillData;
    [SerializeField, Reference("")] private SkillDisplayer skillDisplayer;
    [SerializeField, Reference("Image_Cooldown")] private Image cooldownFillImage;
    [SerializeField, Reference("Text_ChargeCount")] private TMP_Text chargeCountText;
    [SerializeField, Reference("Image_ChargeProgress")] private Image chargeProgressFillImage;
    [SerializeField, Reference("Image_Dim")] private Image dimImage;

    private RPGSkill skill;

    private void OnDestroy()
    {
        UnregisterHandlers();
    }

    public void Setup(EntitySkillSystem skillSystem)
    {
        UnregisterHandlers();
        skillSystem.TryGetSkillByID(skillData.ID, out skill);
        RegisterHandlers();
        UpdateSkillDisplay();
        UpdateCooldownImage();
        UpdateChargeDisplay();
    }

    private void RegisterHandlers()
    {
        if (skill == null)
        {
            return;
        }

        skill.OnCooldownChanged += UpdateCooldownImage;
        skill.OnUpgradeChanged += HandleOnUpgradeChanged;

        if (skill is ICharagable charagable)
        {
            charagable.OnChargeChanged += UpdateChargeDisplay;
        }
    }

    private void UnregisterHandlers()
    {
        if (skill == null)
        {
            return;
        }

        skill.OnCooldownChanged -= UpdateCooldownImage;
        skill.OnUpgradeChanged -= HandleOnUpgradeChanged;
        if (skill is ICharagable charagable)
        {
            charagable.OnChargeChanged -= UpdateChargeDisplay;
        }
    }

    private bool IsSkillValid()
    {
        return skill != null && skill.IsUnlocked();
    }

    private void UpdateSkillDisplay()
    {
        if (!IsSkillValid())
        {
            skillDisplayer.Cleanup();
            return;
        }

        skillDisplayer.Setup(skillData);
    }

    private void UpdateCooldownImage()
    {
        if (!IsSkillValid())
        {
            cooldownFillImage.fillAmount = 0;
            return;
        }

        cooldownFillImage.fillAmount = skill.CooldownFraction;
    }

    private void UpdateChargeDisplay()
    {
        if (!IsSkillValid() || skill is not ICharagable charagable)
        {
            chargeCountText.text = "";
            chargeProgressFillImage.fillAmount = 0;
            return;
        }

        chargeCountText.text = charagable.CurrentCharge.ToString();
        chargeProgressFillImage.fillAmount = 1.0f - charagable.ChargeProgress;
        dimImage.gameObject.SetActive(charagable.CurrentCharge == 0);
    }

    private void HandleOnUpgradeChanged()
    {
        UpdateSkillDisplay();
        UpdateCooldownImage();
        UpdateChargeDisplay();
    }
}