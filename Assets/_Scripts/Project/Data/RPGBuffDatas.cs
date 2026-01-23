using System;

[Serializable]
public abstract class RPGBuffData
{
    public abstract RPGBuff CreateBuff(ICombatable owner = null);
}

[Serializable]
public class IncreaseDamageData : RPGBuffData
{
    public float DamageIncreasePercentage;
    public float Duration;

    public override RPGBuff CreateBuff(ICombatable owner = null)
    {
        return new IncreaseDamage(Duration, DamageIncreasePercentage, owner);
    }
}

[Serializable]
public class IncreaseAttackSpeedData : RPGBuffData
{
    public float AttackSpeedIncreasePercentage;
    public float Duration;

    public override RPGBuff CreateBuff(ICombatable owner = null)
    {
        return new IncreaseAttackSpeed(Duration, AttackSpeedIncreasePercentage, owner);
    }
}
