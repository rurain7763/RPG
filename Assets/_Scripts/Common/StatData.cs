using UnityEngine;

[CreateAssetMenu(menuName = "Common/Stat Data", fileName = "New Stat Data")]
public class StatData : ScriptableObject
{
    public UUID Id;
    public string DisplayName;
    [TextArea] public string Description;
    public Sprite Icon;
    public bool IsPercent;
    public float MinValue;
    public float MaxValue;

    private void Awake()
    {
        Id.Generate();
    }
}