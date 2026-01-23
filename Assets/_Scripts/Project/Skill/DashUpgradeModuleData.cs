using UnityEngine;

[CreateAssetMenu(menuName = "Project/Skill/Dash Upgrade Module Data", fileName = "New Dash Upgrade Module Data")]
public class DashUpgradeModuleData : RPGSkillUpgradeModuleData
{
    public DashUpgradeFlag UpgradeFlag;

    public override uint FlagValue => (uint)UpgradeFlag;

    public override SkillModule CreateModule()
    {
        return new DashUpgradeModule(this);
    }
}

