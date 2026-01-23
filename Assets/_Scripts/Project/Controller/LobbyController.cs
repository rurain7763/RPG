using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviour
{
    private LobbyUI lobbyUI;

    private Coroutine playCoroutine;

    private void Start()
    {
        lobbyUI = RPG.UISys.OpenStatic<LobbyUI>();

        var setupData = new LobbyUISetupData
        {
            OnClickPlay = Play,
            OnClickOption = Option,
            OnClickQuit = Quit,
        };

        lobbyUI.Setup(setupData);

        RPG.AudioSys.SetBGMVolume(RPG.UserDataSys.PlayData.Options.BGMVolume);
        RPG.AudioSys.SetSFXVolume(RPG.UserDataSys.PlayData.Options.SFXVolume);
        RPG.AudioSys.PlayBGM(Local.GetBGMPath(BGMID.Lobby));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (RPG.UISys.AnyPopupActive())
            {
                RPG.UISys.CloseTopPopup();
            }
        }
    }

    private void Play()
    {
        if (playCoroutine != null)
        {
            return;
        }

        playCoroutine = StartCoroutine(PlayCo());
    }

    private IEnumerator PlayCo()
    {
        var asyncOp = SceneManager.LoadSceneAsync("InGame");
        asyncOp.allowSceneActivation = false;

        bool fadeCompleted = false;
        
        RPG.ScreenEffectSys.FadeOut(0.5f, () => { fadeCompleted = true; });
        while (!fadeCompleted)
        {
            yield return null;
        }

        while (!asyncOp.isDone)
        {
            if (asyncOp.progress >= 0.9f)
            {
                break;
            }
            yield return null;
        }

        RPG.UISys.CloseAllPopups();
        RPG.UISys.CloseAllStatics();

        asyncOp.allowSceneActivation = true;
    }

    private void Option()
    {
        var option = RPG.UISys.OpenPopup<OptionUI>();
        option.Setup();
    }

    private void Quit()
    {
        Helper.QuitApplication();
    }
}
