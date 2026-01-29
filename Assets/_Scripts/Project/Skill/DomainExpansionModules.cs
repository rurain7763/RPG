public class DomainExpansionUpgradeModule : SkillModule
{
    public new DomainExpansionUpgradeModuleData Data => base.Data as DomainExpansionUpgradeModuleData;

    public DomainExpansionUpgradeModule(DomainExpansionUpgradeModuleData data)
        : base(data)
    {
    }

    public override void Apply(Skill skill)
    {
        if (skill is not DomainExpansion actual)
        {
            throw new System.InvalidOperationException($"SkillModule {Data.DisplayName} is not compatible with skill {skill.Data.DisplayName}.");
        }

        actual.AddUpgrade(Data.UpgradeFlag);
    }
}
