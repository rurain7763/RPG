using System.Collections.Generic;

public class MaterialItem : Item
{
    public new MaterialItemData ItemData => itemData as MaterialItemData;

    public MaterialItem(MaterialItemData itemData) 
        : base(itemData)
    {
    }

    public MaterialItem(MaterialItemData itemData, SerialNumber serialNumber) 
        : base(itemData, serialNumber)
    {
    }
}

public class EquipmentItem : Item
{
    public new EquipmentItemData ItemData => itemData as EquipmentItemData;

    private List<(StatData, ValueModifier)> statModifiers = new();
    private ItemEffect uniqueEffect;

    private Entity user;

    public bool IsEquipped => user != null;
    public IReadOnlyList<(StatData, ValueModifier)> StatModifiers => statModifiers.AsReadOnly();

    public EquipmentItem(EquipmentItemData itemData) 
        : base(itemData)
    {
        SetupModifiers();
        SetupUniqueEffect();
    }

    public EquipmentItem(EquipmentItemData itemData, SerialNumber serialNumber) 
        : base(itemData, serialNumber)
    {
        SetupModifiers();
        SetupUniqueEffect();
    }

    private void SetupModifiers()
    {
        foreach (var statModData in ItemData.StatModifiers)
        {
            var newMod = statModData.CreateModifier();
            statModifiers.Add((statModData.StatData, newMod));
        }
    }

    private void SetupUniqueEffect()
    {
        if (ItemData.UniqueEffect != null)
        {
            uniqueEffect = ItemData.UniqueEffect.CreateEffect();
        }
    }

    public void Equip(Entity user)
    {
        if (user is not ICombatable combatable)
        {
            return;
        }

        foreach (var pair in statModifiers)
        {
            var stat = combatable.StatSystem.GetStat<Stat>(pair.Item1);
            if (stat == null)
            {
                continue;
            }

            stat.AddModifier(pair.Item2);
        }

        if (uniqueEffect != null)
        {
            uniqueEffect.ApplyEffect(user);
        }

        this.user = user;
    }

    public void Unequip()
    {
        if (user == null)
        {
            return;
        }

        if (uniqueEffect != null)
        {
            uniqueEffect.RemoveEffect();
        }

        var combatable = user as ICombatable;

        foreach (var statMod in statModifiers)
        {
            var stat = combatable.StatSystem.GetStat<Stat>(statMod.Item1);
            if (stat == null)
            {
                continue;
            }

            stat.RemoveModifier(statMod.Item2);
        }

        user = null;
    }
}

public class ConsumableItem : Item
{
    public new ConsumableItemData ItemData => itemData as ConsumableItemData;

    private ItemEffect[] effects;

    public ConsumableItem(ConsumableItemData itemData) 
        : base(itemData)
    {
        SetupEffects();
    }

    public ConsumableItem(ConsumableItemData itemData, SerialNumber serialNumber) 
        : base(itemData, serialNumber)
    {
        SetupEffects();
    }

    private void SetupEffects()
    {
        effects = new ItemEffect[ItemData.Effects.Length];
        for (int i = 0; i < ItemData.Effects.Length; i++)
        {
            effects[i] = ItemData.Effects[i].CreateEffect();
        }
    }

    public void Consume(Entity entity)
    {
        foreach (var effect in effects)
        {
            effect.ApplyEffect(entity);
        }
    }
}