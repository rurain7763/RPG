using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGLevel : Level
{
    [SerializeField] private LevelID levelID;
    [SerializeField] private LevelID nearestTownLevelID;
    [SerializeField] protected Player player;
    [SerializeField] protected BGMID defaultBgmID;

    private HashSet<Entity> entities = new();
    private List<Entity> toAddEntities = new();
    private List<Entity> toRemoveEntities = new();

    public LevelID LevelID => levelID;
    public LevelID NearestTownLevelID => nearestTownLevelID;
    public RPGLevelEnvironment Environment => levelSystem.Environment as RPGLevelEnvironment;

    public override void Enter()
    {
        RPG.LocalPlayer = player;
        Environment.ApplySpawnPolicy(this, player);
        player.Possess(RPG.UserDataSys.CurrentUUID);
        player.Begin();
        player.CombatSystem.OnDie += OnPlayerDie;

        foreach (var entity in GetComponentsInChildren<Entity>())
        {
            if (entity != player)
            {
                entity.Begin();
            }
        }

        RPG.UserDataSys.OnBeforeSaveUserDatas += OnBeforeSaveUserDatas;
    }

    public override void Execute(float deltaTime)
    {
        foreach (var entity in entities)
        {
            if (entity == null)
            {
                toRemoveEntities.Add(entity);
                continue;
            }

            entity.Tick(deltaTime);
        }

        foreach (var entity in toAddEntities)
        {
            entity.transform.SetParent(transform, true);
            entities.Add(entity);
        }
        toAddEntities.Clear();

        foreach (var entity in toRemoveEntities)
        {
            entities.Remove(entity);

            if (entity != null)
            {
                entity.transform.SetParent(null, true);
            }
        }
        toRemoveEntities.Clear();
    }

    public override void Exit()
    {
        RPG.UserDataSys.OnBeforeSaveUserDatas -= OnBeforeSaveUserDatas;

        entities.Remove(player);
        player.CombatSystem.OnDie -= OnPlayerDie;
        player.Unpossess();
    }

    private void OnBeforeSaveUserDatas()
    {
        var progress = RPG.UserDataSys.PlayData.Progress;
        progress.SetProgress(levelID, player.transform.position);
    }

    public void RegisterEntity(Entity entity)
    {
        toAddEntities.Add(entity);
    }

    public void UnregisterEntity(Entity entity)
    {
        toRemoveEntities.Add(entity);
    }

    public void PlayDefaultBGM()
    {
        Environment.PlayBGM(defaultBgmID);
    }

    private void OnPlayerDie()
    {
        var ui = RPG.UISys.OpenPopup<DeathUI>();
        ui.Setup(this, player);
    }

    [System.Diagnostics.Conditional("DEV_BUILD")]
    protected void HandleDevInput()
    {
        // die player immediately
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            var damage = new Damage(player.CombatSystem);
            damage.ElementType = ElementType.Physical;
            damage.PhysicalAmount = 999999;

            player.CombatSystem.TakeDamage(damage);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            RPG.UserDataSys.PlayData.Gold += 1000;
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            RPG.UserDataSys.PlayData.SkillSys.SkillPoints += 1;
        }
    }

    protected void HandleUIInputs()
    {
        player.BlockInput = RPG.UISys.TopPopup != null;

        HandlePopupInput<PlayerUI>(KeyCode.I, (popup) => popup.Setup(player));
        HandlePopupInput<SkillTreeUI>(KeyCode.P, (popup) => popup.Setup(player));
        HandlePopupInput<QuestUI>(KeyCode.Q, (popup) => popup.Setup(player.QuestSystem));
        HandleEscapeInput();
    }

    private void HandlePopupInput<T>(KeyCode keyCode, Action<T> callback) where T : PopupUI
    {
        if (!Input.GetKeyDown(keyCode))
        {
            return;
        }

        var firstPopup = RPG.UISys.GetFirstActivePopup<T>();
        if (firstPopup == null)
        {
            firstPopup = RPG.UISys.OpenPopup<T>();
            callback(firstPopup);
        }
        else
        {
            RPG.UISys.MakePopupToTop(firstPopup);
        }
    }

    private void HandleEscapeInput()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        if (RPG.UISys.AnyPopupActive())
        {
            RPG.UISys.CloseTopPopup();
        }
        else
        {
            var popup = RPG.UISys.OpenPopup<OptionUI>();
            popup.Setup();
        }
    }

    private void OnValidate()
    {
        levelName = $"{levelID}";
    }
}
