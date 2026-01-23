using UnityEngine;

[CreateAssetMenu(fileName = "TimeEchoData", menuName = "Project/Skill/TimeEchoData")]
public class TimeEchoData : SkillCoreData
{
    public Echo EchoPrefab;
    public float EchoDuration = 5f;
    public int MaxEchoAttackCount = 3;
    public float ChanceToDuplicate = 0.3f;
    public Wisp HealingWispPrefab;
    public Wisp CooldownWispPrefab;
    public Wisp CleanseWispPrefab;

    public override Skill CreateSkill() => new TimeEcho(this);
}