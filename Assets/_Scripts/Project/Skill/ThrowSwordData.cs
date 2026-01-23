using UnityEngine;

[CreateAssetMenu(menuName = "Project/Skill/Throw sword Data", fileName = "New Throw sword Skill Data")]
public class ThrowSwordData : SkillCoreData
{
    public ThrowingSword SwordPrefab;
    public float ThrowForce = 10f;
    public int PierceCount = 0;
    public float MaxThrowDistance = -1f;
    public float SpinDuration = 2f;
    public int BounceCount = 0;
    public GameObject TrajectoryDotPrefab;
    public int TrajectoryDotCount = 10;

    public override Skill CreateSkill() => new ThrowSword(this);
}