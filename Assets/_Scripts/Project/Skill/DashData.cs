using UnityEngine;

[CreateAssetMenu(menuName = "Project/Skill/Dash Data", fileName = "New Dash Skill Data")]
public class DashData : SkillCoreData
{
    public Shard ShardPrefab;
    public float DashSpeed = 35f;
    public float DashDuration = 0.1f;

    public override Skill CreateSkill() => new Dash(this);
}