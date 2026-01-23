using UnityEngine;

[CreateAssetMenu(fileName = "MagicBallRainData", menuName = "Project/Skill/MagicBallRainData")]
public class MagicBallRainData : SkillCoreData
{
    public MagicBall MagicBallPrefab;
    public float ArcHeight = 5f;

    public override Skill CreateSkill() => new MagicBallRain(this);
}