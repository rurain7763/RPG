public abstract class SkillModule
{
    public readonly SkillModuleData Data;

    public SkillModule(SkillModuleData data)
    {
        Data = data;
    }

    public bool IsCompatibleWith(SkillCoreData coreData)
    {
        return Data.Core.ID == coreData.ID;
    }

    public abstract void Apply(Skill skill);
}