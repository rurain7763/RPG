using UnityEngine;

[CreateAssetMenu(fileName = "TimeEchoUpgradeModuleData", menuName = "Project/Skill/TimeEchoUpgradeModuleData")]
public class TimeEchoUpgradeModuleData : RPGSkillUpgradeModuleData
{
    public TimeEchoUpgradeFlag UpgradeFlag;

    public override uint FlagValue => (uint)UpgradeFlag;

    public override SkillModule CreateModule()
    {
        return new TimeEchoUpgradeModule(this);
    }
}