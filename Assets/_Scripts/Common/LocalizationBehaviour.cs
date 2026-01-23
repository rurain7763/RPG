using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizationBehaviour : MonoBehaviour
{
    [SerializeField] private bool detectLanguageChange = true;

    private TMP_Text tmpText;
    private LocalizationSystem localizationSys;

    private string originalText;

    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
        originalText = tmpText.text;
    }

    private void OnEnable()
    {
        localizationSys = AppManager.Instance.GetSystem<LocalizationSystem>();
        if (localizationSys == null)
        {
            Logger.Warn("LocallizationDataTable is null!");
            return;
        }

        if (detectLanguageChange)
        {
            localizationSys.OnLanguageChanged += OnLanguageChanged;
        }

        SetLocalizationText(originalText);
    }

    private void OnDisable()
    {
        if (localizationSys == null)
        {
            return;
        }

        if (detectLanguageChange)
        {
            localizationSys.OnLanguageChanged -= OnLanguageChanged;
        }
    }

    private void OnLanguageChanged()
    {
        SetLocalizationText(originalText);
    }

    public void SetLocalizationText(string text)
    {
        originalText = text;

        if (localizationSys == null)
        {
            tmpText.text = text;
            return;
        }

#if DEV_BUILD
        if (!IsValidString(text))
        {
            Logger.Warn($"Invalid locallization string: {text}");
            tmpText.text = text;
            return;
        }
#endif

        tmpText.text = LocalizeString(text);
    }

    private bool IsValidString(string text)
    {
        int stk = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '{')
            {
                stk++;
                i++;
            }
            else if (text[i] == '}')
            {
                stk--;
                if (stk < 0)
                {
                    return false;
                }
            }
        }

        return stk == 0;
    }

    private string LocalizeString(string text)
    {
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '{')
            {
                int endIndex = text.IndexOf('}', i);
 
                string key = text.Substring(i + 1, endIndex - i - 1);
                
                if (localizationSys.TryGetLocalizedText(key, out var localizedText))
                {
                    builder.Append(localizedText);
                }
                else
                {
                    builder.Append($"{{{key}}}");
                }

                i = endIndex;
            }
            else
            {
                builder.Append(text[i]);
            }
        }

        return builder.ToString();
    }
}