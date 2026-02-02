using System;
using UnityEngine;

public class ProgressSystem
{
    public LevelID LastLevelID { get; set; }
    public Vector2 LastPosition { get; set; } = Vector2.zero;
    public bool HasLastPosition => LastPosition != Vector2.zero;
    public float RemainHealth { get; set; } = float.MaxValue;

    public ProgressSystem()
    {
    }

    public ProgressSystem(ProgressSystemDTO data)
    {
        LastLevelID = data.LastLevelID;
        LastPosition = data.LastPosition;
        RemainHealth = data.RemainHealth;
    }

    public ProgressSystemDTO CaptureDTO()
    {
        return new ProgressSystemDTO
        {
            LastLevelID = LastLevelID,
            LastPosition = LastPosition,
            RemainHealth = RemainHealth
        };
    }
}

[Serializable]
public class ProgressSystemDTO
{
    public LevelID LastLevelID;
    public Vector2 LastPosition = Vector2.zero;
    public float RemainHealth = float.MaxValue;
}