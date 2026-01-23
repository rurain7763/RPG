using UnityEngine;

[CreateAssetMenu(fileName = "SpellDeathThunderData", menuName = "Project/Skill/SpellDeathThunderData")]
public class SpellDeathThunderData : SkillCoreData
{
    public DeathThunder DeathThunderPrefab;
    public int MaxSpellCount = 3;

    public override Skill CreateSkill() => new SpellDeathThunder(this);
}