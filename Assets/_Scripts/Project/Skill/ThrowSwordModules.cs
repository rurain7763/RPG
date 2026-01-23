public class ThrowSwordUpgradeModule : SkillModule
{
    public new ThrowSwordUpgradeModuleData Data => base.Data as ThrowSwordUpgradeModuleData;

    public ThrowSwordUpgradeModule(ThrowSwordUpgradeModuleData data)
        : base(data)
    {
    }

    public override void Apply(Skill skill)
    {
        if (skill is not ThrowSword actual)
        {
            throw new System.InvalidOperationException($"SkillModule {Data.DisplayName} is not compatible with skill {skill.Data.DisplayName}.");
        }

        actual.AddUpgrade(Data.UpgradeFlag);
    }
}