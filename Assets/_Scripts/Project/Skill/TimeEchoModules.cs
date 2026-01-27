public class TimeEchoUpgradeModule : SkillModule
{
    public new TimeEchoUpgradeModuleData Data => base.Data as TimeEchoUpgradeModuleData;

    public TimeEchoUpgradeModule(TimeEchoUpgradeModuleData data)
        : base(data)
    {
    }

    public override void Apply(Skill skill)
    {
        if (skill is not TimeEcho actual)
        {
            throw new System.InvalidOperationException($"SkillModule {Data.DisplayName} is not compatible with skill {skill.Data.DisplayName}.");
        }

        actual.AddUpgrade(Data.UpgradeFlag);
    }
}