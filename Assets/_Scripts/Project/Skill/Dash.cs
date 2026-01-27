using System;
using UnityEngine;

[Serializable]
public class Dash : RPGSkill    
{
    public new DashData Data => base.Data as DashData;

    private Rigidbody2D rigidBody;

    private float backupGravityScale;
    private float startTime;

    public Dash(DashData data) 
        : base(data)
    {
        // TODO: Full Unlock upgrade for test
        //AddUpgrade(SkillUpgradeFlag.Default | SkillUpgradeFlag.Dash_DejaVu);
        //AddUpgrade(SkillUpgradeFlag.Default | SkillUpgradeFlag.Dash_ShardOnStart | SkillUpgradeFlag.Dash_ShardOnEnd);
    }

    protected override void StartUse(GameObject user, Arguments parameters = null)
    {
        base.StartUse(user, parameters);

        rigidBody = user.GetComponentInChildren<Rigidbody2D>();

        float dashDirection = entity.MovementAxis.x != 0 ? Mathf.Sign(entity.MovementAxis.x) : entity.FacingDirection;
        rigidBody.linearVelocity = new Vector2(dashDirection * Data.DashSpeed, 0);

        backupGravityScale = rigidBody.gravityScale;

        if (HasUpgrade(DashUpgradeFlag.ShardOnStart))
        {
            var shard = GameObject.Instantiate(Data.ShardPrefab, entity.CenterPosition, Quaternion.identity);
            shard.Owner = entity;
        }

        rigidBody.gravityScale = 0;
        startTime = Time.time;

        var vfxSystem = user.GetComponentInChildren<EntityVFXSystem>();
        if (vfxSystem != null)
        {
            vfxSystem.SpawnImageEchoVFX(echoCount: 5, duration: Data.DashDuration);
        }
    }

    public override bool IsComplete()
    {
        return base.IsComplete() && (Time.time - startTime >= Data.DashDuration || entity.IsWallInFront());
    }

    public override void Cleanup()
    {
        if (HasUpgrade(DashUpgradeFlag.ShardOnEnd))
        {
            var shard = GameObject.Instantiate(Data.ShardPrefab, entity.CenterPosition, Quaternion.identity);
            shard.Owner = entity;
        }

        rigidBody.gravityScale = backupGravityScale;
        rigidBody.linearVelocity = new Vector2(0, rigidBody.linearVelocity.y);
    }

    public override bool CanUse(GameObject user)
    {
        return base.CanUse(user) && Helper.HasComponentInChildren<Rigidbody2D>(user);
    }
}