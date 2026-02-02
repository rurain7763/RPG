using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Data", menuName = "Project/NPC Data")]
public class NPCData : ScriptableObject
{
    public RPGSpeaker Speaker;
    public string[] Dialogues;

    public string GetRandomDialogue()
    {
        if (Dialogues == null || Dialogues.Length == 0)
        {
            return string.Empty;
        }

        int randomIndex = Random.Range(0, Dialogues.Length);
        return Dialogues[randomIndex];
    }
}