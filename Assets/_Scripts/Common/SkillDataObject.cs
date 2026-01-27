using UnityEngine;

public enum SkillCategory
{
    Active,
    Passive
}

public abstract class SkillDataObject : ScriptableObject
{
    public UUID ID;
    public string DisplayName;
    [TextArea] public string Description;
    public Sprite Icon;

    protected void OnValidate()
    {
        if (!ID.IsValid())
        {
            ID.Generate();
        }
    }
}

public abstract class SkillCoreData : SkillDataObject
{
    public SkillCategory Category;
    public float BaseCooldown;

    public abstract Skill CreateSkill();
}

public abstract class SkillModuleData : SkillDataObject
{
    public SkillCoreData Core;

    public abstract SkillModule CreateModule();
}