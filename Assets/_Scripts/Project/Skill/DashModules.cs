public class DashUpgradeModule : SkillModule
{
    public new DashUpgradeModuleData Data => base.Data as DashUpgradeModuleData;

    public DashUpgradeModule(DashUpgradeModuleData data)
        : base(data)
    {
    }

    public override void Apply(Skill skill)
    {
        if (skill is not Dash dashSkill)
        {
            throw new System.InvalidOperationException($"SkillModule {Data.DisplayName} is not compatible with skill {skill.Data.DisplayName}.");
        }

        dashSkill.AddUpgrade(Data.UpgradeFlag);
    }
}
