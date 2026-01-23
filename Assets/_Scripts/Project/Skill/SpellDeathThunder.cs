using System;
using UnityEngine;

public class SpellDeathThunderParameters : Arguments
{
    public readonly Entity Target;

    public SpellDeathThunderParameters(Entity target)
    {
        Target = target;
    }
}

[Serializable]
public class SpellDeathThunder : RPGSkill
{
    public new SpellDeathThunderData Data => base.Data as SpellDeathThunderData;

    private Entity target;

    public int MaxSpellCount => Data.MaxSpellCount;
    public int SpellCount { get; private set; }

    public SpellDeathThunder(SpellDeathThunderData data) 
        : base(data)
    {
        RegisterEventHandler("Cast", HandleCastEvent);
    }

    protected override void StartUse(GameObject user, Arguments parameters = null)
    {
        base.StartUse(user, parameters);

        if (parameters is not SpellDeathThunderParameters actual)
        {
            Logger.Warn("SpellDeathThunder skill requires SpellDeathThunderParameters.");
            return;
        }
        
        if (actual.Target is not ICombatable)
        {
            Logger.Warn("SpellDeathThunder skill requires target to be ICombatable.");
            return;
        }

        target = actual.Target;
        SpellCount = 0;
    }

    public override bool IsComplete()
    {
        return base.IsComplete() && SpellCount >= Data.MaxSpellCount;
    }

    private void HandleCastEvent(IEventData data)
    {
        var combatable = entity as ICombatable;

        var deathThunder = GameObject.Instantiate(Data.DeathThunderPrefab, target.transform.position, Quaternion.identity);
        deathThunder.Setup(combatable);

        SpellCount++;
    }
}