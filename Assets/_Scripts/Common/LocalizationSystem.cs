using System;
using System.Collections.Generic;
using UnityEngine;

public enum Language
{
    English,
    Spanish,
    French,
    German,
    Chinese,
    Japanese,
    Korean,
    Russian
}

public class LocalizationSystem : AppSystem
{
    private string localizationFileMemory = string.Empty;

    private Language currentLanguage;
    private Dictionary<string, string> localizedTexts = new();

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
            LoadLocalizedTexts();

            OnLanguageChanged?.Invoke();
        }
    }

    public event Action OnLanguageChanged;

    public override void OnAttach(AppManager appManager)
    {
        base.OnAttach(appManager);

        var resourcesSystem = appManager.GetSystem<ResourcesSystem>();

        if (!resourcesSystem.TryGetResource($"Localization", out TextAsset csvAsset))
        {
            Logger.Warn($"Localization file for language not found.");
            return;
        }

        localizationFileMemory = csvAsset.text;
        currentLanguage = Language.English;
        LoadLocalizedTexts();
    }

    private void LoadLocalizedTexts()
    {
        if (localizationFileMemory == string.Empty)
        {
            return;
        }

        localizedTexts.Clear();

        var csv = CSVReader.ReadFromMemory(localizationFileMemory);

        string columnKey = currentLanguage.ToString();
        columnKey = columnKey.ToLower();

        foreach (var row in csv)
        {
            string key = Convert.ToString(row["key"]);
            string value = Convert.ToString(row[columnKey]);

            if (localizedTexts.ContainsKey(key))
            {
                Logger.Warn($"Duplicate localization key found: {key}");
                continue;
            }

            localizedTexts[key] = value;
        }
    }

    public bool TryGetLocalizedText(string key, out string value)
    {
        return localizedTexts.TryGetValue(key, out value);
    }

    public override Type[] GetDependencySystemTypes()
    {
        return new Type[]
        {
            typeof(ResourcesSystem)
        };
    }
}