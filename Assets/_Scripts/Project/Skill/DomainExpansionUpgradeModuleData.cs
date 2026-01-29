using UnityEngine;

[CreateAssetMenu(menuName = "Project/Skill/DomainExpansion Upgrade Module Data", fileName = "New DomainExpansion Upgrade Module Data")]
public class DomainExpansionUpgradeModuleData : RPGSkillUpgradeModuleData
{
    public DomainExpansionUpgradeFlag UpgradeFlag;

    public override uint FlagValue => (uint)UpgradeFlag;

    public override SkillModule CreateModule()
    {
        return new DomainExpansionUpgradeModule(this);
    }
}

