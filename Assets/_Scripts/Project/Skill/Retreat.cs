using System;
using UnityEngine;

public class RetreatParameters : Arguments
{
    public readonly Vector2 RetreatDirection;

    public RetreatParameters(Vector2 retreatDirection)
    {
        RetreatDirection = retreatDirection;
    }
}

[Serializable]
public class Retreat : RPGSkill
{
    public new RetreatData Data => base.Data as RetreatData;

    private Rigidbody2D rb;

    private float backupGravityScale;
    private float startTime;

    public Retreat(RetreatData data) 
        : base(data)
    {
    }

    protected override void StartUse(GameObject user, Arguments parameters = null)
    {
        base.StartUse(user, parameters);

        RetreatParameters retreatParams = parameters as RetreatParameters;
        if (retreatParams == null)
        {
            Debug.LogError("Retreat skill requires RetreatParameters.");
            return;
        }

        entity.LookAt(entity.CenterPosition - retreatParams.RetreatDirection);

        rb = user.GetComponentInChildren<Rigidbody2D>();
        backupGravityScale = rb.gravityScale;
        rb.gravityScale = 0f;
        startTime = Time.time;
        rb.linearVelocity = retreatParams.RetreatDirection * Data.RetreatSpeed;

        var vfxSystem = user.GetComponentInChildren<EntityVFXSystem>();
        vfxSystem?.SpawnImageEchoVFX(5, Data.Duration);
    }

    public override void Cleanup()
    {
        rb.gravityScale = backupGravityScale;
        rb.linearVelocity = Vector2.zero;
    }

    public override bool IsComplete()
    {
        return base.IsComplete() && (Time.time - startTime >= Data.Duration || entity.IsWallBehind() || entity.IsCliffBehind());
    }

    public override bool CanUse(GameObject user)
    {
        return base.CanUse(user) && Helper.HasComponentInChildren<Rigidbody2D>(user);
    }
}