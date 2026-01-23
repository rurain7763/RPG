using UnityEngine;

[CreateAssetMenu(fileName = "TimeShardData", menuName = "Project/Skill/TimeShardData")]
public class TimeShardData : SkillCoreData
{
    public Shard ShardPrefab;
    public int BaseChargeCount;

    public override Skill CreateSkill() => new TimeShard(this);
}