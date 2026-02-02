using Unity.Cinemachine;
using UnityEngine;

public class RPGLevelEnvironment : LevelEnvironment
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Canvas inGameUICanvas;
    [SerializeField] private ParallaxBackground2D parallaxBackground;
    [SerializeField] private HpBarPool hpBarPool;
    [SerializeField] private PooledObjectPool dialogueTextPool;
    [SerializeField] private PooledObjectPool questIndicatorPool;

    private BGMID bgmID;

    public Camera MainCamera => mainCamera;
    public Canvas InGameUICanvas => inGameUICanvas;
    public ISpawnPolicy SpawnPolicy { get; set; }
    public ParallaxBackground2D ParallaxBackground => parallaxBackground;
    public HpBarPool HpBarPool => hpBarPool;
    public PooledObjectPool DialogueTextPool => dialogueTextPool;
    public PooledObjectPool QuestIndicatorPool => questIndicatorPool;

    private void Awake()
    {
        hpBarPool.Initialize();
        dialogueTextPool.Initialize();
        questIndicatorPool.Initialize();
    }

    public void SetCamera(Transform anchor)
    {
        if (cinemachineCamera == null)
        {
            Logger.Warn("CinemachineCamera is not assigned in RPGLevelEnvironment.");
            return;
        }

        cinemachineCamera.Follow = anchor;
        cinemachineCamera.OnTargetObjectWarped(anchor, anchor.position - cinemachineCamera.transform.position);
    }

    public void SetBackground()
    {
        parallaxBackground.FollowTarget = mainCamera.transform;

        // TODO: set background images from arguments near future
    }

    public void ApplySpawnPolicy(RPGLevel level, Player player)
    {
        if (SpawnPolicy == null)
        {
            return;
        }

        SpawnPolicy.Apply(level, player);
    }

    public void PlayBGM(BGMID next)
    {
        if (bgmID == next)
        {
            return;
        }

        RPG.AudioSys.PlayBGMCrossFade(Local.GetBGMPath(next));
        bgmID = next;
    }
}

public interface ISpawnPolicy
{
    void Apply(RPGLevel level, Player player);
}

public class StartPositionSpawnPolicy : ISpawnPolicy
{
    public void Apply(RPGLevel level, Player player)
    {
        player.transform.position = level.PlayerStartPosition;
    }
}

public class CheckpointSpawnPolicy : ISpawnPolicy
{
    public void Apply(RPGLevel level, Player player)
    {
        var playDataTable = RPG.UserDataSys.GetTable<UserPlayDataTable>(player.UserID);
        if (playDataTable == null)
        {
            Logger.Warn($"Failed to retrieve UserPlayDataTable for user ID {player.UserID}");
            return;
        }

        foreach (var checkpoint in level.GetComponentsInChildren<Checkpoint>())
        {
            if (checkpoint.CheckpointID == playDataTable.Checkpoint.LastCheckpointID)
            {
                player.transform.position = checkpoint.transform.position;
                return; 
            }
        }

        Logger.Warn("No last checkpoint found in the level for CheckpointSpawnPolicy.");
        player.transform.position = level.PlayerStartPosition;
    }
}

public class PortalSpawnPolicy : ISpawnPolicy
{
    public void Apply(RPGLevel level, Player player)
    {
        var allPortals = level.GetComponentsInChildren<Portal>();
        if (allPortals.Length > 0)
        {
            player.transform.position = allPortals[0].transform.position;
        }
        else
        {
            Logger.Warn("No portals found in the level for PortalSpawnPolicy.");
            player.transform.position = level.PlayerStartPosition;
        }
    }
}

public class SpecificPositionSpawnPolicy : ISpawnPolicy
{
    private Vector2 position;

    public SpecificPositionSpawnPolicy(Vector2 position)
    {
        this.position = position;
    }

    public void Apply(RPGLevel level, Player player)
    {
        player.transform.position = position;
    }
}
