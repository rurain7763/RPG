public class TimeShardUpgradeModule : SkillModule
{
    public new TimeShardUpgradeModuleData Data => base.Data as TimeShardUpgradeModuleData;

    public TimeShardUpgradeModule(TimeShardUpgradeModuleData data) 
        : base(data)
    {
    }

    public override void Apply(Skill skill)
    {
        if (skill is not TimeShard timeShard)
        {
            throw new System.InvalidOperationException($"SkillModule {Data.DisplayName} is not compatible with skill {skill.Data.DisplayName}.");
        }

        timeShard.AddUpgrade(Data.UpgradeFlag);
    }
}