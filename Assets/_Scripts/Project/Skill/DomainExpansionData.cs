using UnityEngine;

[CreateAssetMenu(menuName = "Project/Skill/Domain Expansion Data", fileName = "New Domain Expansion Skill Data")]
public class DomainExpansionData : SkillCoreData
{
    public Territory TerritoryPrefab;
    public float TargetSize = 5f;
    public float ExpansionDuration = 1f;
    public float ActiveDuration = 5f;

    public Shard ShardPrefab;
    public float ShardSpamInterval = 0.5f;

    public Echo EchoPrefab;
    public float EchoSpamInterval = 0.5f;

    public override Skill CreateSkill() => new DomainExpansion(this);
}
