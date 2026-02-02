using UnityEngine;

public class InGameController : MonoBehaviour
{
    private void Start()
    {
        var progresss = RPG.UserDataSys.PlayData.Progress;

        ISpawnPolicy spawnPolicy = null;
        if (progresss.HasLastPosition)
        {
            spawnPolicy = new SpecificPositionSpawnPolicy(progresss.LastPosition);
        }
        else
        {
            spawnPolicy = new StartPositionSpawnPolicy();
        }

        RPG.LoadLevel(progresss.LastLevelID, spawnPolicy);
    }
}
