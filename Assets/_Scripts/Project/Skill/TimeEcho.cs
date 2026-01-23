using System;
using UnityEngine;

[Serializable]
public class TimeEcho : RPGSkill
{
    public new TimeEchoData Data => base.Data as TimeEchoData;

    private Echo currentEcho;

    public TimeEcho(SkillCoreData data) 
        : base(data)
    {
        // TODO: Full unlock upgrade for test
        AddUpgrade(SkillUpgradeFlag.Default 
            | SkillUpgradeFlag.TimeEcho_ChanceToMultipleEchoes 
            | SkillUpgradeFlag.TimeEcho_MultiAttack 
            | SkillUpgradeFlag.TimeEcho_HealWisp
            | SkillUpgradeFlag.TimeEcho_CooldownWisp
            | SkillUpgradeFlag.TimeEcho_CleanseWisp);
    }

    public override void Tick(float delta)
    {
        base.Tick(delta);

        if (currentEcho != null)
        {
            currentEcho.Tick(delta);
        }
    }

    protected override void StartUse(GameObject user, Arguments parameters)
    {
        base.StartUse(user, parameters);

        currentEcho = CreateEcho();
        currentEcho.transform.position = user.transform.position;
        currentEcho.SetFacing(entity.IsFacingRight);

        if (HasUpgrade(SkillUpgradeFlag.TimeEcho_ChanceToMultipleEchoes))
        {
            currentEcho.OnExpired += TryDuplicateEcho;
        }
    }

    private Echo CreateEcho()
    {
        var echo = GameObject.Instantiate(Data.EchoPrefab);
        echo.Duration = Data.EchoDuration;
        echo.MaxAttackCount = 0;

        if (HasUpgrade(SkillUpgradeFlag.TimeEcho_SingleAttack))
        {
            echo.MaxAttackCount = 1;
        }
        
        if (HasUpgrade(SkillUpgradeFlag.TimeEcho_MultiAttack))
        {
            echo.MaxAttackCount = Data.MaxEchoAttackCount;
        }

        if (HasUpgrade(SkillUpgradeFlag.TimeEcho_HealWisp))
        {
            echo.OnExpired += () =>
            {
                var wisp = GameObject.Instantiate(Data.HealingWispPrefab, echo.CenterPosition, Quaternion.identity);
                wisp.Owner = entity;
                wisp.Target = entity;
            };
        }

        if (HasUpgrade(SkillUpgradeFlag.TimeEcho_CooldownWisp))
        {
            echo.OnExpired += () =>
            {
                var wisp = GameObject.Instantiate(Data.CooldownWispPrefab, echo.CenterPosition, Quaternion.identity);
                wisp.Owner = entity;
                wisp.Target = entity;
            };
        }

        if (HasUpgrade(SkillUpgradeFlag.TimeEcho_CleanseWisp))
        {
            echo.OnExpired += () =>
            {
                var wisp = GameObject.Instantiate(Data.CleanseWispPrefab, echo.CenterPosition, Quaternion.identity);
                wisp.Owner = entity;
                wisp.Target = entity;
            };
        }

        echo.Begin();

        return echo;
    }

    private void TryDuplicateEcho()
    {
        if(UnityEngine.Random.value >= Data.ChanceToDuplicate)
        {
            return;
        }

        var targets = currentEcho.TargetDetector.DetectTargets();
        
        Entity nextTarget = null;
        float closestDistanceSqr = float.MaxValue;
        foreach (var target in currentEcho.LastHitTargets)
        {
            if (target == null)
            {
                continue;
            }

            ICombatable combatable = target as ICombatable;
            if (combatable.CombatSystem.IsDead)
            {
                continue;
            }

            float distanceSqr = (target.transform.position - currentEcho.transform.position).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                nextTarget = target;
            }
        }

        if (nextTarget == null)
        {
            return;
        }

        float centerToTargetDetectorDist = Mathf.Abs(currentEcho.TargetDetector.transform.position.x - currentEcho.CenterPosition.x);
        Vector2 backPosition = nextTarget.transform.position - new Vector3(centerToTargetDetectorDist * nextTarget.FacingDirection, 0f, 0f);

        currentEcho = CreateEcho();
        currentEcho.transform.position = backPosition;
        currentEcho.SetFacing(nextTarget.IsFacingRight);
    }
}
