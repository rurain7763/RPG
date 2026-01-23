using System;
using UnityEngine;

public class MagicBallRainParameters : Arguments
{
    public readonly Entity TargetEntity;

    public MagicBallRainParameters(Entity TargetEntity)
    {
        this.TargetEntity = TargetEntity;
    }
}

[Serializable]
public class MagicBallRain : RPGSkill
{
    public new MagicBallRainData Data => base.Data as MagicBallRainData;

    private Entity targetEntity;
    private bool complete;

    public MagicBallRain(SkillCoreData data) 
        : base(data)
    {
        RegisterEventHandler("Cast", HandleCastEvent);
        RegisterEventHandler("End", HandleEndEvent);
    }

    protected override void StartUse(GameObject user, Arguments parameters = null)
    {
        base.StartUse(user, parameters);

        MagicBallRainParameters rainParams = parameters as MagicBallRainParameters;
        if (rainParams == null)
        {
            Logger.Warn("MagicBallRain skill requires MagicBallRainParameters.");
            return;
        }

        targetEntity = rainParams.TargetEntity;
        complete = false;
    }

    public override bool IsComplete()
    {
        return base.IsComplete() && complete;
    }

    private void HandleCastEvent(IEventData data)
    {
        if (data is not StringEventData strData)
        {
            Logger.Warn("Invalid event data for Cast event in MagicBallRain skill.");
            return;
        }

        Transform castPoint = entity.transform.Find(strData.Value);
        if (castPoint == null)
        {
            Logger.Warn($"Cast point '{strData.Value}' not found on entity.");
            return;
        }

        if (entity is not ICombatable combatable)
        {
            return;
        }

        MagicBall magicBall = GameObject.Instantiate(Data.MagicBallPrefab, castPoint.position, Quaternion.identity);
        Vector2 arcVelocity = Helper.CalcArcVelocity2D(castPoint.position, targetEntity.CenterPosition, Data.ArcHeight, Physics2D.gravity.y * magicBall.RigidBody.gravityScale);
        magicBall.Setup(combatable);
        magicBall.Fire(arcVelocity);
    }

    private void HandleEndEvent(IEventData data)
    {
        complete = true;
    }
}