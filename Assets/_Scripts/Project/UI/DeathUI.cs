using UnityEngine;
using UnityEngine.UI;

public class DeathUI : PopupUI
{
    [SerializeField, Reference("Popup/Button_GoToTown")] private Button goToTownButton;
    [SerializeField, Reference("Popup/Button_GoToCheckpoint")] private Button goToCheckpoint;

    private Player player;
    private RPGLevel currentLevel;

    private void Awake()
    {
        goToTownButton.onClick.AddListener(OnClickGoToTownButton);
        goToCheckpoint.onClick.AddListener(OnClickGoToCheckpointButton);
    }

    public void Setup(RPGLevel currentLevel, Player player)
    {
        this.currentLevel = currentLevel;
        this.player = player;

        goToCheckpoint.interactable = RPG.HasLastCheckpointInLevel(currentLevel, player);
    }

    private void OnClickGoToTownButton()
    {
        RPG.LoadLevel(currentLevel.NearestTownLevelID);
        CloseThis();
    }

    private void OnClickGoToCheckpointButton()
    {
        RPG.TeleportPlayerToLastCheckpoint(currentLevel, player);
        CloseThis();
    }
}