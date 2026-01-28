using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.WSA;

public class OptionUI : PopupUI
{
    [SerializeField, Reference("Popup/BGMVolumeSlot/Slider")] private Slider bgmVolumeSlider;
    [SerializeField, Reference("Popup/SFXVolumeSlot/Slider")] private Slider sfxVolumeSlider;
    [SerializeField, Reference("Popup/LanguageSlot/Dropdown")] private TMP_Dropdown languageDropdown;

    private Language[] indexToLanguage = new[] 
    { 
        Language.English, 
        Language.Korean 
    };

    private void Awake()
    {
        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    public void Setup()
    {
        bgmVolumeSlider.value = RPG.AudioSys.BGMVolume;
        sfxVolumeSlider.value = RPG.AudioSys.SFXVolume;

        var currentLanguage = RPG.LocalizationSys.CurrentLanguage;
        for (int i = 0; i < indexToLanguage.Length; i++)
        {
            if (indexToLanguage[i] == currentLanguage)
            {
                languageDropdown.value = i;
                break;
            }
        }
    }

    private void OnBGMVolumeChanged(float value)
    {
        RPG.UserDataSys.PlayData.Options.BGMVolume = value;
        RPG.AudioSys.SetBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        RPG.UserDataSys.PlayData.Options.SFXVolume = value;
        RPG.AudioSys.SetSFXVolume(value);
    }

    private void OnLanguageChanged(int index)
    {
        RPG.LocalizationSys.CurrentLanguage = indexToLanguage[index];
    }
}
