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

        goToCheckpoint.interactable = GetLastCheckpointInLevel(currentLevel, player) != null;
    }

    private Checkpoint GetLastCheckpointInLevel(RPGLevel level, Player player)
    {
        var playDataTable = RPG.UserDataSys.GetTable<UserPlayDataTable>(player.UserID);
        if (playDataTable == null)
        {
            Logger.Warn($"Failed to retrieve UserPlayDataTable for user ID {player.UserID}");
            return null;
        }

        foreach (var checkpoint in level.GetComponentsInChildren<Checkpoint>())
        {
            if (checkpoint.CheckpointID == playDataTable.Checkpoint.LastCheckpointID)
            {
                return checkpoint;
            }
        }

        return null;
    }

    private void OnClickGoToTownButton()
    {
        RPG.LoadLevel(currentLevel.NearestTownLevelID);
        CloseThis();
    }

    private void OnClickGoToCheckpointButton()
    {
        var lastCheckpoint = GetLastCheckpointInLevel(currentLevel, player);
        player.transform.position = lastCheckpoint.transform.position;
        CloseThis();
    }
}