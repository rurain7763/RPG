using UnityEngine;

public struct ProgressSystemInitData
{
    public LevelID LastLevelID;
    public Vector2 LastPosition;
}

public class ProgressSystem
{
    private LevelID lastLevelID;
    private Vector2 lastPosition;

    public LevelID LastLevelID => lastLevelID;
    public Vector2 LastPosition => lastPosition;

    public ProgressSystem(ProgressSystemInitData? initData = null)
    {
        if (initData.HasValue)
        {
            var data = initData.Value;

            lastLevelID = data.LastLevelID;
            lastPosition = data.LastPosition;
        }
    }

    public void SetProgress(LevelID levelID, Vector2 position)
    {
        lastLevelID = levelID;
        lastPosition = position;
    }
}