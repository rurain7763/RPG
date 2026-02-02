using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDUI : StaticUI
{
    [SerializeField, Reference("Text_Level")] private TMP_Text levelText;
    [SerializeField, Reference("Hpbar")] private HpBar hpBar;
    [SerializeField, Reference("Expbar")] private Slider expBar;
    [SerializeField, Reference("Expbar/Text_Value")] private TMP_Text expBarTextValue;
    [SerializeField, Reference()] private QuickItemEquipSlotDisplayer[] quickItemEquipSlotDisplayers;
    [SerializeField, Reference()] private SkillEquipSlotDisplayer[] skillEquipSlotDisplayers;
    [SerializeField, Reference("EnemyHpDisplayers")] EnemyHpDisplayer[] enemyHpDisplayers;

    private Player player;
    private int nextEnemyHpDisplayerIndex = 0;

    public override void OnOpen(Transform parent)
    {
        base.OnOpen(parent);

        foreach(var displayer in enemyHpDisplayers)
        {
            displayer.gameObject.SetActive(false);
        }
    }

    public override void OnClose(Transform parent, Action onCompleteClose = null)
    {
        base.OnClose(parent, onCompleteClose);

        if (player != null)
        {
            player.CombatSystem.OnHealthChanged -= UpdateHpBar;
            player.PlayLevelSystem.OnExpChanged -= HandleExpChanged;
        }
    }

    public void Setup(Player player)
    {
        this.player = player;

        player.CombatSystem.OnHealthChanged += UpdateHpBar;
        player.PlayLevelSystem.OnExpChanged += HandleExpChanged;

        foreach (var quickItemEquipSlotDisplayer in quickItemEquipSlotDisplayers)
        {
            quickItemEquipSlotDisplayer.Setup(player.QuickItemSystem);
        }

        foreach (var skillEquipSlotDisplayer in skillEquipSlotDisplayers)
        {
            skillEquipSlotDisplayer.Setup(player.SkillSystem);
        }

        UpdateLevelText();
        UpdateHpBar();
        UpdateExpBar();
    }

    private void UpdateLevelText()
    {
        levelText.text = $"Lv. {player.PlayLevelSystem.CurrentLevel}";
    }

    private void UpdateHpBar()
    {
        hpBar.SetHp(player.CombatSystem.CurrentHealth, player.CombatSystem.MaxHealth);
    }

    private void UpdateExpBar()
    {
        if (player.PlayLevelSystem.CurrentLevel >= RPG.MaxPlayerLevel)
        {
            expBar.value = 1f;
            return;
        }

        expBar.value = (float)player.PlayLevelSystem.CurrentExp / player.PlayLevelSystem.ExpToReachNextLevel;
        expBarTextValue.text = $"{player.PlayLevelSystem.CurrentExp} / {player.PlayLevelSystem.ExpToReachNextLevel}";
    }

    private void HandleExpChanged()
    {
        UpdateLevelText();
        UpdateExpBar();
    }

    public void RegisterHpDisplayer(string name, ICombatable combatable)
    {
        if (nextEnemyHpDisplayerIndex >= enemyHpDisplayers.Length)
        {
            Logger.Warn("No more available EnemyHpDisplayers to register.");
            return;
        }

        var displayer = enemyHpDisplayers[nextEnemyHpDisplayerIndex];
        displayer.Begin(combatable, name);
        nextEnemyHpDisplayerIndex++;

        displayer.gameObject.SetActive(true);
    }

    public void UnregisterHpDisplayer(ICombatable combatable)
    {
        int index = Array.FindIndex(enemyHpDisplayers, d => d.Owner == combatable);
        if (index == -1)
        {
            Logger.Warn("EnemyHpDisplayer for the given combatable not found.");
            return;
        }

        for (int i = index; i < nextEnemyHpDisplayerIndex - 1; i++)
        {
            var temp = enemyHpDisplayers[i];
            enemyHpDisplayers[i] = enemyHpDisplayers[i + 1];
            enemyHpDisplayers[i + 1] = temp;
        }

        for (int i = index; i < nextEnemyHpDisplayerIndex - 1; i++)
        {
            enemyHpDisplayers[i].transform.SetSiblingIndex(i);
        }

        nextEnemyHpDisplayerIndex--;
        enemyHpDisplayers[nextEnemyHpDisplayerIndex].End();
        enemyHpDisplayers[nextEnemyHpDisplayerIndex].gameObject.SetActive(false);
    }
}
