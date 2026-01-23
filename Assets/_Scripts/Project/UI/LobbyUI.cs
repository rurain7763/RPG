using System;
using UnityEngine;
using UnityEngine.UI;

public struct LobbyUISetupData
{
    public Action OnClickPlay;
    public Action OnClickOption;
    public Action OnClickQuit;
}

public class LobbyUI : StaticUI
{
    [SerializeField, Reference("Button_Play")] private Button playButton;
    [SerializeField, Reference("Button_Option")] private Button optionButton;
    [SerializeField, Reference("Button_Quit")] private Button quitButton;

    private Action onClickPlay;
    private Action onClickOption;
    private Action onClickQuit;

    private void Awake()
    {
        playButton.onClick.AddListener(OnClickPlayButton);
        optionButton.onClick.AddListener(OnClickOptionButton);
        quitButton.onClick.AddListener(OnClickQuitButton);
    }

    public void Setup(LobbyUISetupData data)
    {
        onClickPlay = data.OnClickPlay;
        onClickOption = data.OnClickOption;
        onClickQuit = data.OnClickQuit;
    }

    private void OnClickPlayButton()
    {
        RPG.AudioSys.PlaySFX(Local.GetSFXPath(SFXID.ButtonClick));
        onClickPlay?.Invoke();
    }

    private void OnClickOptionButton()
    {
        RPG.AudioSys.PlaySFX(Local.GetSFXPath(SFXID.ButtonClick));
        onClickOption?.Invoke();
    }

    private void OnClickQuitButton()
    {
        RPG.AudioSys.PlaySFX(Local.GetSFXPath(SFXID.ButtonClick));
        onClickQuit?.Invoke();
    }
}