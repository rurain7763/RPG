using UnityEngine;

[CreateAssetMenu(fileName = "BlinkData", menuName = "Project/Skill/BlinkData")]
public class BlinkData : SkillCoreData
{
    public int MaxFindSafePointAttempts = 10;
    public LayerMask GroundMask;
    public LayerMask ObstacleMask;

    public override Skill CreateSkill() => new Blink(this);
}