using UnityEngine;

public class InGameController : MonoBehaviour
{
    private void Start()
    {
        var progresss = RPG.UserDataSys.PlayData.Progress;
        RPG.LoadLevel(progresss.LastLevelID, new SpecificPositionSpawnPolicy(progresss.LastPosition));
    }
}
