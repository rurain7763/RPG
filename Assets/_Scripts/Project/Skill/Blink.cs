using System;
using UnityEngine;

public class BlinkParameters : Arguments
{
    public readonly Rect TestArea;
    public readonly Vector2 UserOverlapSize;

    public BlinkParameters(Rect testArea, Vector2 userOverlapSize)
    {
        TestArea = testArea;
        UserOverlapSize = userOverlapSize;
    }
}

[Serializable]
public class Blink : RPGSkill
{
    public new BlinkData Data => base.Data as BlinkData;

    private bool foundSafePoint;
    private Vector2 safePoint;
    private bool isDone;

    public Blink(BlinkData data) 
        : base(data)
    {
        RegisterEventHandler("Blink", HandleBlinkEvent);
    }

    protected override void StartUse(GameObject user, Arguments parameters = null)
    {
        base.StartUse(user, parameters);
        BlinkParameters blinkParams = parameters as BlinkParameters;
        if (blinkParams == null)
        {
            Logger.Warn("Blink skill requires BlinkParameters.");
            return;
        }

        foundSafePoint = FindSafePoint(blinkParams.TestArea, blinkParams.UserOverlapSize, out safePoint);
        isDone = false;
    }

    private bool FindSafePoint(Rect testArea, Vector2 overlapSize, out Vector2 safePoint)
    {        
        safePoint = Vector2.zero;
        for (int i = 0; i < Data.MaxFindSafePointAttempts; i++)
        {
            Vector2 origin = new Vector2(UnityEngine.Random.Range(testArea.xMin, testArea.xMax), testArea.yMax);

            if (Physics2D.OverlapPoint(origin, Data.GroundMask | Data.ObstacleMask) != null)
            {
                continue;
            }

            var hits = Physics2D.RaycastAll(origin, Vector2.down, testArea.height, Data.GroundMask);
            if (hits.Length == 0)
            {
                continue;   
            }

            int randomIndex = UnityEngine.Random.Range(0, hits.Length);

            var hit = hits[randomIndex];

            var centerPoint = hit.point + Vector2.up * (overlapSize.y * 0.5f);

            Collider2D[] obstacles = Physics2D.OverlapBoxAll(centerPoint, overlapSize, 0, Data.ObstacleMask);
            bool blocked = false;
            foreach (var obstacle in obstacles)
            {
                if (obstacle != hit.collider)
                {
                    blocked = true;
                    break;
                }
            }

            if (blocked)
            {
                continue;
            }

            safePoint = hit.point;

            return true;
        }

        return false;
    }

    public override bool IsComplete()
    {
        return base.IsComplete() && isDone;
    }

    private void HandleBlinkEvent(IEventData data)
    {
        if (foundSafePoint)
        {
            entity.transform.position = safePoint;
        }

        isDone = true;
    }
}