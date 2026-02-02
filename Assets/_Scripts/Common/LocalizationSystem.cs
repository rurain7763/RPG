using System;
using TMPro;
using UnityEngine;

public class LocalizationSystem : AppSystem
{
    [SerializeReference, SubclassSelector] private ILanguageSource languageSource;
    [SerializeField] private SerializedDictionary<Language, TMP_FontAsset> languageFonts;
    [SerializeField] private Language currentLanguage;

    public Language CurrentLanguage
    {
        get => currentLanguage;
        set
        {
            if (currentLanguage == value)
            {
                return;
            }

            currentLanguage = value;
            languageSource.SetLanguage(currentLanguage);

            OnLanguageChanged?.Invoke();
        }
    }

    public TMP_FontAsset CurrentFont => languageFonts[currentLanguage];

    public event Action OnLanguageChanged;

    public override void OnAttach(AppManager appManager)
    {
        languageSource.SetLanguage(currentLanguage);
    }

    public bool TryGetLocalizedText(string key, out string value)
    {
        return languageSource.TryGetText(key, out value);
    }
}