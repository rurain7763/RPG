using UnityEngine;

[CreateAssetMenu(fileName = "TimeShardUpgradeModuleData", menuName = "Project/Skill/TimeShardUpgradeModuleData")]
public class TimeShardUpgradeModuleData : RPGSkillUpgradeModuleData
{
    public TimeShardUpgradeFlag UpgradeFlag;
    public override uint FlagValue => (uint)UpgradeFlag;

    public override SkillModule CreateModule()
    {
        return new TimeShardUpgradeModule(this);
    }
}