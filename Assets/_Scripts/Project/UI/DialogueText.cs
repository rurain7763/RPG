using UnityEngine;

public class DialogueText : MonoBehaviour
{
    private LocalizationText textText;
    private WorldToUIFollower worldToUIFollower;
    private TypewriterTextEffector typewriterTextEffector;

    private void Awake()
    {
        textText = GetComponentInChildren<LocalizationText>();
        worldToUIFollower = GetComponentInChildren<WorldToUIFollower>();
        typewriterTextEffector = GetComponentInChildren<TypewriterTextEffector>();
    }

    public void SetText(string text, bool useTypewriterEffect = false)
    {
        textText.SetText(text);

        if (useTypewriterEffect)
        {
            typewriterTextEffector.Play();
        }
    }

    public void SetAnchor(Transform anchor)
    {
        worldToUIFollower.SetAnchor(anchor);
    }
}