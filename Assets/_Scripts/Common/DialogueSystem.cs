using UnityEngine;
using Ink.Runtime;
using System.Collections.Generic;
using System;
using System.Linq;

public class DialogueSystem : AppSystem
{
    [SerializeField] private SerializedDictionary<string, TextAsset> dialogueFiles;

    private Dictionary<UUID, DialogueSpeaker> dialogueSpeakers = new();

    public override void OnAttach(AppManager appManager)
    {
        var resourcesSystem = appManager.GetSystem<ResourcesSystem>();

        resourcesSystem.EachAllScriptableObject<DialogueSpeaker>((speaker) =>
        {
            dialogueSpeakers[speaker.ID] = speaker;
        });
    }

    public Dialogue CreateDialogue(string storyName, string knotName, DialogueSpeaker triggerSpeaker = null)
    {
        if (!dialogueFiles.TryGetValue(storyName, out TextAsset storyFile))
        {
            Logger.Error($"Story '{storyName}' not found in story files.");
            return null;
        }

        Story newStory = new Story(storyFile.text);
        newStory.ChoosePathString(knotName);

        return new Dialogue(newStory, triggerSpeaker);
    }

    public T GetDialogueSpeaker<T>(UUID speakerID) where T : DialogueSpeaker
    {
        if (dialogueSpeakers.TryGetValue(speakerID, out DialogueSpeaker speaker))
        {
            return speaker as T;
        }
        return null;
    }

    public T GetDialogueSpeaker<T>(string alias) where T : DialogueSpeaker
    {
        var speaker = dialogueSpeakers.Values.FirstOrDefault(speaker => speaker.Alias == alias);
        return speaker as T;
    }

    public override Type[] GetDependencySystemTypes()
    {
        return new Type[]
        {
            typeof(ResourcesSystem)
        };
    }
}
