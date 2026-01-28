using System;
using System.Collections.Generic;
using UnityEngine;

public interface ILanguageSource
{
    void SetLanguage(Language key);
    bool TryGetText(string key, out string value);
}

[Serializable]
public class CSVLanguageSource : ILanguageSource
{
    [SerializeField] private TextAsset csvAsset;

    private Dictionary<string, string> localizedTexts = new();

    public CSVLanguageSource() { }

    public CSVLanguageSource(TextAsset csvAsset)
    {
        this.csvAsset = csvAsset;
    }

    public void SetLanguage(Language lang)
    {
        localizedTexts.Clear();

        if (csvAsset == null)
        {
            Logger.Error("CSV asset is not assigned.");
            return;
        }

        var csv = CSVReader.ReadFromMemory(csvAsset.text);
        string columnKey = lang.ToString().ToLower();
        foreach (var row in csv)
        {
            string key = Convert.ToString(row["key"]);
            string value = Convert.ToString(row[columnKey]);

            if (!string.IsNullOrEmpty(key))
            {
                if (localizedTexts.ContainsKey(key))
                {
                    Logger.Warn($"Duplicate key '{key}' found in localization CSV.");
                }
                else
                {
                    localizedTexts[key] = value ?? string.Empty;
                }
            }
        }
    }

    public bool TryGetText(string key, out string value)
    {
        return localizedTexts.TryGetValue(key, out value);
    }
}