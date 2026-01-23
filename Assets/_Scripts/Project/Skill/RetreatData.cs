using UnityEngine;

[CreateAssetMenu(fileName = "RetreatData", menuName = "Project/Skill/RetreatData")]
public class RetreatData : SkillCoreData
{
    public float RetreatSpeed = 15f;
    public float Duration = 0.5f;

    public override Skill CreateSkill() => new Retreat(this);
}