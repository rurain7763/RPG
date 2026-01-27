using JetBrains.Annotations;
using System;
using UnityEngine;

public interface ICombatable
{
    public EntityCombatSystem CombatSystem { get; }
    public EntityStatSystem StatSystem { get; }
    public BuffSystem BuffSystem { get; }
}

public interface ISkillUser
{
    public EntitySkillSystem SkillSystem { get; }
}

public interface IHasInventory
{
    public InventorySystem InventorySystem { get; }
}

public interface IInteractable
{
    void Interact(Player player);
}

public enum LevelID
{
    Level_0,
    Level_1,
    Level_2,
}

public enum VFXID
{
    Hit,
    IceHit,
    FireHit,
    CriticalHit,
    LightningHit,
    LightningStrike,
    ImageEcho,
    ShardExplosion,
    Smoke,
    IceBlast,
}

public enum BGMID
{
    Lobby,
    Level_0,
    Reaper
}

public enum SFXID
{
    SwordHit,
    SwordMiss,
    FireBurn,
    ButtonClick,
}

public enum ElementType
{
    Physical,
    Fire,
    Ice,
    Lightning,
}

public enum BuffID : uint
{
    Frozen,
    Burning,
    Electrified,
    IncreaseDamage,
    Slow,
    IncreaseAttackSpeed,
}

[Flags]
public enum SkillUpgradeCommonFlag : uint
{
    Default = 1 << 0,
}

[Flags]
public enum DashUpgradeFlag : uint
{
    CloneOnStart = 1 << 1,
    CloneOnEnd = 1 << 2,
    ShardOnStart = 1 << 3,
    ShardOnEnd = 1 << 4
}

[Flags]
public enum TimeShardUpgradeFlag : uint
{
    MoveToEnemey = 1 << 1,
    Multicast = 1 << 2,
    Teleport = 1 << 3,
    TeleportHpRewind = 1 << 4
}

[Flags]
public enum ThrowSwordUpgradeFlag : uint
{
    Bounce = 1 << 1,
    Pierce = 1 << 2,
    Spin = 1 << 3
}

[Flags]
public enum TimeEchoUpgradeFlag : uint
{
    SingleAttack = 1 << 1,
    MultiAttack = 1 << 2,
    ChanceToMultipleEchoes = 1 << 3,
    HealWisp = 1 << 4,
    CleanseWisp = 1 << 5,
    CooldownWisp = 1 << 6,
}

public enum StatType
{
    Health,
    HealthRegeneration,
    Strength,
    Agility,
    Intelligence,
    Vitality,
    Damage,
    CritPower,
    CritRate,
    ArmorReduction,
    FireDamage,
    IceDamage,
    LightningDamage,
    Armor,
    Evasion,
    FireResistance,
    IceResistance,
    LightningResistance,
    MoveSpeed,
    AttackSpeed
}

public enum ItemCategory
{
    Material,
    Equipment,
    Consumable,
}

public enum EquipmentCategory
{
    Weapon,
    Armor,
    Trinket,
}

public enum EquipmentSlotType
{
    Weapon,
    Armor,
    FirstTrinket,
    SecondTrinket,
}

public enum CraftItemCategory
{
    Weapon,
}

public enum Mood
{
    Neutral,
    Happy,
    Sad,
    Angry,
    Surprised
}

[Serializable]
public struct StatModifierData
{
    public StatData StatData;
    [SerializeReference, SubclassSelector] public ValueModifierData ModifierData;

    public ValueModifier CreateModifier()
    {
        return ModifierData.CreateModifier();
    }
}

public class InventorySlotDragAndDropPayload : IDragAndDropPayload
{
    public InventorySlot InventorySlot { get; private set; }

    public InventorySlotDragAndDropPayload(InventorySlot inventorySlot)
    {
        InventorySlot = inventorySlot;
    }
}

[Serializable]
public class ItemDTO
{
    public UUID ID;
    public SerialNumber SerialNumber;
}

[Serializable]
public class EquippedItemDTO
{
    public EquipmentSlotType SlotType;
    public ItemDTO Item;
}

[Serializable]
public class PortalSystemDTO
{
    public bool Launched;
    public LevelID TargetLevelID;
    public Vector2 Position;
    public bool DirectionRight;
}

[Serializable]
public class ProgressSystemDTO
{
    public LevelID LastLevelID;
    public Vector2 LastPosition;
}

[Serializable]
public class OptionsSystemDTO
{
    public float BGMVolume = 1f;
    public float SFXVolume = 1f;
}

[Serializable]
public class UserPlayDataDTO
{
    public ulong Gold;
    public ItemDTO[] Items = new ItemDTO[0];
    public ItemDTO[] StorageItems = new ItemDTO[0];
    public EquippedItemDTO[] EquippedItems = new EquippedItemDTO[0];
    public QuestSystemDTO QuestSystemData = new();
    public UUID LastCheckpointID;
    public PortalSystemDTO PortalSystemData = new();
    public ProgressSystemDTO ProgressData = new();
    public OptionsSystemDTO OptionsData = new();
    public SkillSystemDTO SkillSystemData = new();
}

public static class Local
{
    public static string GetLevelName(LevelID id)
    {
        return $"{id}";
    }

    public static string GetVFXPath(VFXID id)
    {
        return $"{id}VFX";
    }

    public static string GetBGMPath(BGMID id)
    {
        return $"{id}BGM";
    }

    public static string GetSFXPath(SFXID id)
    {
        return $"{id}SFX";
    }
}