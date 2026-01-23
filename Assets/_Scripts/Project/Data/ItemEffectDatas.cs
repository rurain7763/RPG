using System;
using UnityEngine;

[Serializable]
public abstract class ItemEffectData
{
    public abstract ItemEffect CreateEffect();
}

[Serializable]
public class SpawnPortalItemEffectData : ItemEffectData
{
    public override ItemEffect CreateEffect()
    {
        return new SpawnPortalEffect();
    }
}

[Serializable]
public class HealItemEffectData : ItemEffectData
{
    public float HealRate;

    public override ItemEffect CreateEffect()
    {
        return new HealItemEffect(HealRate);
    }
}

[Serializable]
public class ApplyBuffItemEffectData : ItemEffectData
{
    [SerializeReference, SubclassSelector] public RPGBuffData BuffData;

    public override ItemEffect CreateEffect()
    {
        return new ApplyBuffItemEffect(BuffData);
    }
}

[Serializable]
public class IceBlastItemEffectData : ItemEffectData
{
    public float HealthPercentageToTrigger;
    public float Cooldown;
    public float reflectDamageRate;
    public VFXID VFXToSpawn;

    public override ItemEffect CreateEffect()
    {
        return new IceBlastItemEffect(HealthPercentageToTrigger, reflectDamageRate, Cooldown, VFXToSpawn);
    }
}

[Serializable]
public class DrainDamageItemEffectData : ItemEffectData
{
    public float DrainRate;
    
    public override ItemEffect CreateEffect()
    {
        return new DrainDamageItemEffect(DrainRate);
    }
}