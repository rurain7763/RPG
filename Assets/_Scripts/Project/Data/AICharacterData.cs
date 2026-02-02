using UnityEngine;

[CreateAssetMenu(fileName = "New AI Character Data", menuName = "Project/AI Character Data")]
public class AICharacterData : ScriptableObject
{
    public int baseExpReward;
    public AnimationCurve expMultiplierCurve;

    public int GetExp(int level)
    {
        if (level < 1)
        {
            return 0;
        }
        float multiplier = expMultiplierCurve.Evaluate(level);
        return Mathf.RoundToInt(baseExpReward * multiplier);
    }
}