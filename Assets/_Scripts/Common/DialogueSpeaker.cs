using UnityEngine;

public abstract class DialogueSpeaker : ScriptableObject
{
    [SerializeField] private UUID id;
    [SerializeField] private string alias;
    [SerializeField] private string displayName;

    public UUID ID => id;
    public string Alias => alias;
    public string DisplayName => displayName;

    private void OnValidate()
    {
        if (!id.IsValid())
        {
            id.Generate();
        }
    }
}