using System;
using System.Collections;
using UnityEngine;

public class RPG : AppManager
{
    public static readonly int MaxPlayerInventorySlots = 200;
    public static readonly int MaxPlayerStorageInventorySlots = 500;
    public static readonly int MaxMerchantItemCount = 50;

    public static readonly float ArmorConstant = 100f;

    public static readonly int MaxPlayerLevel = 100;

    public static UISystem UISys { get; private set; }
    public static ScreenEffectSystem ScreenEffectSys { get; private set; }
    public static ResourcesSystem ResSys { get; private set; }
    public static VFXSystem VFXSys { get; private set; }
    public static AudioSystem AudioSys { get; private set; }
    public static RPGAppDataSystem AppDataSys { get; private set; }
    public static RPGUserDataSystem UserDataSys { get; private set; }
    public static LevelSystem LevelSys { get; private set; }
    public static DialogueSystem DialogueSys { get; private set; }
    public static LocalizationSystem LocalizationSys { get; private set; }

    public static EventDispatcher EventDispatcher { get; private set; }

    public static Player LocalPlayer { get; set; }

    private static Coroutine loadLevelCoroutine;

    protected override void Init()
    {
        base.Init();

        ResSys = AttachSystem<ResourcesSystem>();
        UISys = GetSystem<UISystem>();
        ScreenEffectSys = GetSystem<ScreenEffectSystem>();
        VFXSys = GetSystem<VFXSystem>();
        AudioSys = GetSystem<AudioSystem>();
        AppDataSys = GetSystem<RPGAppDataSystem>();
        UserDataSys = GetSystem<RPGUserDataSystem>();
        LevelSys = GetSystem<LevelSystem>();
        DialogueSys = GetSystem<DialogueSystem>();
        LocalizationSys = GetSystem<LocalizationSystem>();

        EventDispatcher = new EventDispatcher();

        AudioSys.sfxMinDistance = 10f;
        AudioSys.sfxMaxDistance = 25f;
    }

    private void Update()
    {
        EventDispatcher.PollEvents();
    }

    public static Damage CalcDamage(ICombatable attacker, ICombatable defender)
    {
        var damage = new Damage(attacker.CombatSystem, defender.CombatSystem);

        damage.PhysicalAmount = attacker.StatSystem.PhysicalDamage.FinalValue;
        damage.IsCritical = attacker.CombatSystem.ChanceToCritical(attacker.StatSystem.TotalCriticalRate.FinalValue);
        if (damage.IsCritical)
        {
            damage.PhysicalAmount = attacker.CombatSystem.CalcCriticalDamage(damage.PhysicalAmount, attacker.StatSystem.TotalCriticalPower.FinalValue);
        }

        ElementType elementType = ElementType.Physical;
        float elementalDamage = 0;
        float elementalResistance = 0f;
        if (attacker.StatSystem.FireDamage.FinalValue > elementalDamage)
        {
            elementType = ElementType.Fire;
            elementalDamage = attacker.StatSystem.FireDamage.FinalValue;
            elementalResistance = defender.StatSystem.FireResistance.FinalValue;
        }

        if (attacker.StatSystem.IceDamage.FinalValue > elementalDamage)
        {
            elementType = ElementType.Ice;
            elementalDamage = attacker.StatSystem.IceDamage.FinalValue;
            elementalResistance = defender.StatSystem.IceResistance.FinalValue;
        }

        if (attacker.StatSystem.LightningDamage.FinalValue > elementalDamage)
        {
            elementType = ElementType.Lightning;
            elementalDamage = attacker.StatSystem.LightningDamage.FinalValue;
            elementalResistance = defender.StatSystem.LightningResistance.FinalValue;
        }

        damage.ElementType = elementType;
        damage.ElementalAmount = elementalDamage;
        damage.PhysicalAmount = attacker.CombatSystem.CalcDamageAfterArmor(damage.PhysicalAmount, defender.StatSystem.TotalArmor.FinalValue, ArmorConstant, attacker.StatSystem.ArmorReduction.FinalValue);
        damage.ElementalAmount = attacker.CombatSystem.CalcDamageAfterResistance(damage.ElementalAmount, elementalResistance);

        return damage;
    }

    public static RPGBuff CalcBuffByDamage(ICombatable attacker, ICombatable defender, Damage damage)
    {
        RPGBuff buff = null;

        if (damage.ElementType == ElementType.Ice)
        {
            buff = attacker.CombatSystem.GetFrozenBuff(2.0f, defender.StatSystem.IceResistance.FinalValue, attacker);
        }
        else if (damage.ElementType == ElementType.Fire)
        {
            buff = attacker.CombatSystem.GetBurningBuff(3.0f, defender.StatSystem.FireResistance.FinalValue, attacker);
        }
        else if (damage.ElementType == ElementType.Lightning)
        {
            buff = attacker.CombatSystem.GetElectrifiedBuff(4.0f, defender.StatSystem.LightningResistance.FinalValue, attacker);
        }

        return buff;
    }

    public static LevelID GetNearestTownLevelFromCurrentLevel()
    {
        var currentLevel = LevelSys.CurrentLevel as RPGLevel;
        return currentLevel.NearestTownLevelID;
    }

    public static void LoadLevel(LevelID levelID, ISpawnPolicy spawnPolicy = null)
    {
        if (loadLevelCoroutine != null)
        {
            Logger.Warn("A level is already being loaded.");
            return;
        }

        loadLevelCoroutine = Instance.StartCoroutine(LoadLevelCo(levelID, spawnPolicy));
    }

    private static IEnumerator LoadLevelCo(LevelID levelID, ISpawnPolicy spawnPolicy)
    {
        bool fadeCompleted = false;

        if (LevelSys.CurrentLevel != null)
        {
            ScreenEffectSys.FadeOut(0.5f, () => { fadeCompleted = true; });

            while (!fadeCompleted)
            {
                yield return null;
            }
        }

        var levelEnvironment = LevelSys.Environment as RPGLevelEnvironment;
        levelEnvironment.SpawnPolicy = spawnPolicy;

        var loadOp = LevelSys.LoadLevelAsync(Local.GetLevelName(levelID));
        loadOp.AllowToNextLevel = false;
        while (loadOp.Progress < 0.9f)
        {
            yield return null;
        }

        loadOp.AllowToNextLevel = true;

        yield return new WaitForSeconds(0.5f);

        ScreenEffectSys.FadeIn(0.5f);

        loadLevelCoroutine = null;
    }

    public static Dialogue CreateQuestDialogue(QuestData questData, Action onAccept = null)
    {
        var dialogue = DialogueSys.CreateDialogue("Quest", $"Quest_{questData.ID}_Offer");
        dialogue.BindExternalFunction("AcceptQuest", onAccept);
        return dialogue;
    }

    public static Dialogue CreateQuestDialogue(Quest quest, Action onAdvance = null, Action onComplete = null)
    {
        string knotName = string.Empty;
        if (quest.CanBeCompleted())
        {
            knotName = $"Quest_{quest.Data.ID}_Complete";
        }
        else if (quest.CanAdvance())
        {
            knotName = $"Quest_{quest.Data.ID}_{quest.CurrentStepIndex}_Complete";
        }
        else
        {
            knotName = $"Quest_{quest.Data.ID}_{quest.CurrentStepIndex}_Progress";
        }

        var dialogue = DialogueSys.CreateDialogue("Quest", knotName);
        dialogue.BindExternalFunction("AdvanceQuest", onAdvance);
        dialogue.BindExternalFunction("CompleteQuest", onComplete);
        return dialogue;
    }
}