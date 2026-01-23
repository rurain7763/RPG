using UnityEngine;

[CreateAssetMenu(fileName = "ThrowSwordUpgradeModuleData", menuName = "Project/Skill/ThrowSwordUpgradeModuleData")]
public class ThrowSwordUpgradeModuleData : RPGSkillUpgradeModuleData
{
    public ThrowSwordUpgradeFlag UpgradeFlag;

    public override uint FlagValue => (uint)UpgradeFlag;

    public override SkillModule CreateModule()
    {
        return new ThrowSwordUpgradeModule(this);
    }
}