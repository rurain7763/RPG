using UnityEngine;

[CreateAssetMenu(fileName = "New RPGSpeaker", menuName = "Project/RPGSpeaker")]
public class RPGSpeaker : DialogueSpeaker
{
    [SerializeField] private SerializedDictionary<Mood, Sprite> portraits;

    public Sprite GetPortrait(Mood mood = Mood.Neutral)
    {
        if (portraits.TryGetValue(mood, out Sprite portrait))
        {
            return portrait;
        }
        return null;
    }
}