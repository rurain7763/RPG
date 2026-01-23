using System;
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

        int randomIndex = UnityEngine.Random.Range(0, Dialogues.Length);
        return Dialogues[randomIndex];
    }

    [ContextMenu("Make Random Dialogue")]
    private void MakeRandomDialogue()
    {
        string[] candidates = new string[]
        {
            "Hello there, traveler!",
            "The weather is nice today, isn't it?",
            "Have you heard the latest news from the capital?",
            "Be careful out there, monsters have been spotted nearby.",
            "If you need supplies, feel free to visit my shop.",
            "Legends say that a hidden treasure lies in the old ruins.",
            "Stay awhile and listen to my stories.",
            "May your journey be safe and prosperous!",
            "The stars are particularly bright tonight.",
            "Remember, courage is the key to overcoming any challenge."
        };

        int numberOfDialogues = UnityEngine.Random.Range(5, 11);

        Dialogues = new string[numberOfDialogues];
        for (int i = 0; i < numberOfDialogues; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, candidates.Length);
            Dialogues[i] = candidates[randomIndex];
        }
    }
}