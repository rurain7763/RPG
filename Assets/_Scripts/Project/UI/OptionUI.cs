using UnityEngine;
using UnityEngine.UI;

public class OptionUI : PopupUI
{
    [SerializeField, Reference("Popup/BGMVolumeSlot/Slider")] private Slider bgmVolumeSlider;
    [SerializeField, Reference("Popup/SFXVolumeSlot/Slider")] private Slider sfxVolumeSlider;

    private void Awake()
    {
        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    public void Setup()
    {
        bgmVolumeSlider.value = RPG.AudioSys.BGMVolume;
        sfxVolumeSlider.value = RPG.AudioSys.SFXVolume;
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
}
