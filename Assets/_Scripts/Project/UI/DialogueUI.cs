using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : PopupUI
{
    [SerializeField, Reference("Popup/Image_Speaker")] private Image speakerImage;
    [SerializeField, Reference("Popup/Image_Speaker/Text_Name")] private TMP_Text speakerNameText;
    [SerializeField, Reference("Popup/DialogueView/Text_Dialogue")] private TMP_Text dialogueText;
    [SerializeField, Reference("Popup/DialogueView/Text_Dialogue")] private TypewriterTextEffector dialogueTextEffector;
    [SerializeField, Reference("Popup/DialogueView/Text_Choices")] private TMP_Text choicesText;
    [SerializeField, Reference("Popup/DialogueView/Text_Choices")] private TypewriterTextEffector choicesTextEffector;

    private Dialogue dialogue;

    private string currentLine = "";

    private List<DialogueChoice> currentChoices = new();
    private int currentChoiceIndex = -1;
    private List<DialogueTag> currentTags = new();

    private RPGSpeaker currentSpeaker;

    private Coroutine playTypewritersCoroutine;

    public void Setup(Dialogue dialogue)
    {
        this.dialogue = dialogue;

        if (this.dialogue.CanContinue)
        {
            dialogue.Continue();
            dialogue.ExecutePendingTasks();
            HandleOnDidContinue();
        }
        else
        {
            CloseThis();
        }
    }

    private void HandleOnDidContinue()
    {
        currentLine = dialogue.CurrentText;
        if (dialogue.HasChoices)
        {
            currentChoices = dialogue.CurrentChoices;
            currentChoiceIndex = 0;
        }
        else
        {
            currentChoices.Clear();
            currentChoiceIndex = -1;
        }

        if (dialogue.HasTags)
        {
            currentTags = dialogue.CurrentTags;
        }
        else
        {
            currentTags.Clear();
        }

        UpdateUIsByTags();
        UpdateDialogueText();
        UpdateChoicesText();
        PlayTypewriters();
    }

    private void Update()
    {
        if (dialogue == null)
        {
            return;
        }

        if (playTypewritersCoroutine != null && Input.anyKeyDown)
        {
            StopTypewriters();
            return;
        }
        
        HandleReturnInput();
        HandleArrowInput();
    }

    private void HandleReturnInput()
    {
        if (!Input.GetKeyDown(KeyCode.Return))
        {
            return;
        }

        if (currentChoices.Count > 0)
        {
            dialogue.ChooseChoiceIndex(currentChoiceIndex);
            currentChoices.Clear();
            currentChoiceIndex = -1;
        }

        if (dialogue.CanContinue)
        {
            dialogue.Continue();
            dialogue.ExecutePendingTasks();

            if (string.IsNullOrEmpty(dialogue.CurrentText))
            {
                CloseThis();
            }
            else
            {
                HandleOnDidContinue();
            }
        }
        else
        {
            CloseThis();
        }
    }

    private void HandleArrowInput()
    {
        if (currentChoices.Count == 0)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentChoiceIndex = Helper.PreviousRepeat(currentChoiceIndex, currentChoices.Count);
            UpdateChoicesText();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentChoiceIndex = Helper.NextRepeat(currentChoiceIndex, currentChoices.Count);
            UpdateChoicesText();
        }
    }

    private void UpdateUIsByTags()
    {
        foreach (var tag in currentTags)
        {
            switch (tag.Key)
            {
                case "speaker":
                    if (tag.Value == "trigger")
                    {
                        currentSpeaker = dialogue.TriggerSpeaker as RPGSpeaker;
                    }
                    else
                    {
                        currentSpeaker = RPG.DialogueSys.GetDialogueSpeaker<RPGSpeaker>(tag.Value);
                    }

                    if (currentSpeaker != null)
                    {
                        speakerImage.sprite = currentSpeaker.GetPortrait();
                        speakerNameText.text = currentSpeaker.DisplayName;
                    }
                    break;
                case "mood":
                    if (currentSpeaker != null)
                    {
                        string enumString = tag.Value;
                        if (System.Enum.TryParse(enumString, out Mood mood))
                        {
                            Sprite moodSprite = currentSpeaker.GetPortrait(mood);
                            speakerImage.sprite = moodSprite;
                        }
                    }
                    break;
            }
        }
    }

    private void UpdateDialogueText()
    {
        dialogueText.text = currentLine;
    }

    private void UpdateChoicesText()
    {
        if (currentChoices.Count == 0)
        {
            choicesText.text = "";
            return;
        }
        var choicesDisplayText = new StringBuilder();
        for (int i = 0; i < currentChoices.Count; i++)
        {
            string lineText = Helper.MakeColoredString((i == currentChoiceIndex) ? Color.yellow : Color.white, $"{i + 1}) {currentChoices[i].Text}");
            choicesDisplayText.AppendLine(lineText);
        }
        choicesText.text = choicesDisplayText.ToString();
    }

    private void PlayTypewriters()
    {
        StopTypewriters();
        playTypewritersCoroutine = StartCoroutine(PlayTypewritersCo());
    }

    private IEnumerator PlayTypewritersCo()
    {
        TMP_Text[] texts = { dialogueText, choicesText };
        TypewriterTextEffector[] effectors = { dialogueTextEffector, choicesTextEffector };

        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].gameObject.SetActive(true);
            effectors[i].Play();

            while (effectors[i].IsPlaying)
            {
                yield return null;
            }
        }

        playTypewritersCoroutine = null;
    }

    private void StopTypewriters()
    {
        if (playTypewritersCoroutine == null)
        {
            return;
        }

        StopCoroutine(playTypewritersCoroutine);
        playTypewritersCoroutine = null;

        TMP_Text[] texts = { dialogueText, choicesText };
        TypewriterTextEffector[] effectors = { dialogueTextEffector, choicesTextEffector };

        for (int i = 0; i < texts.Length; i++)
        {
            effectors[i].Stop();
            texts[i].gameObject.SetActive(true);
        }
    }
}