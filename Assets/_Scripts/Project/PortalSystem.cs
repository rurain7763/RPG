using System;
using UnityEngine;

public struct PortalSystemInitData
{
    public bool Launched;
    public LevelID TargetLevelID;
    public Vector2 Position;
    public bool DirectionRight;
}

public class PortalSystem
{
    private bool launched;
    private LevelID targetLevelID;
    private Vector2 position;
    private bool directionRight;

    public bool Launched => launched;
    public LevelID TargetLevelID => targetLevelID;
    public Vector2 Position => position;
    public bool DirectionRight => directionRight;

    public event Action OnPortalChanged;

    public PortalSystem(PortalSystemInitData? initData = null)
    {
        if (initData.HasValue)
        {
            var data = initData.Value;

            launched = data.Launched;
            targetLevelID = data.TargetLevelID;
            position = data.Position;
            directionRight = data.DirectionRight;
        }
    }

    public void LaunchPortal(LevelID levelID, Vector2 position, bool directionRight)
    {
        launched = true;
        targetLevelID = levelID;
        this.position = position;
        this.directionRight = directionRight;

        OnPortalChanged?.Invoke();
    }

    public void ResetPortal()
    {
        launched = false;

        OnPortalChanged?.Invoke();
    }
}