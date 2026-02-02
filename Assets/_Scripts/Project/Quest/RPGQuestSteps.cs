using System;
using UnityEngine;

[Serializable]
public class TalkToNPCQuestStepData : QuestStepData
{
    public UUID TargetNPCID;
    public override QuestStep CreateStep()
    {
        return new TalkToNPCQuestStep(this);
    }
}

[Serializable]
public class KillEnemyQuestStepData : QuestStepData
{
    [TypeConstraint(typeof(Entity))] public SerializedType EnemyType;
    public int RequiredKillCount;

    public override QuestStep CreateStep()
    {
        return new KillEnemyQuestStep(this);
    }
}

[Serializable]
public class DeliverItemToQuestStepData : QuestStepData
{
    public ItemData Item;
    public int Quantity;
    public UUID TargetID;

    public override QuestStep CreateStep()
    {
        return new DeliverItemToQuestStep(this);
    }
}

public class TalkToNPCQuestStep : QuestStep
{
    public new TalkToNPCQuestStepData Data => base.Data as TalkToNPCQuestStepData;

    public TalkToNPCQuestStep(TalkToNPCQuestStepData data)
        : base(data)
    {
    }

    public override void Begin()
    {
    }

    public override void End()
    {
    }

    public override bool IsAchieved() => true;

    public override float GetProgress() => 1.0f;

    public override string GetProgressText(string format)
    {
        return string.Format(format, Data.Description, 1, 1);
    }

    public override UUID GetObjectiveTarget() => Data.TargetNPCID;
}

public class KillEnemyQuestStep : QuestStep
{
    public new KillEnemyQuestStepData Data => base.Data as KillEnemyQuestStepData;

    private int currentKillCount;

    public KillEnemyQuestStep(KillEnemyQuestStepData data)
        : base(data)
    {
        currentKillCount = 0;
    }

    public override void Begin()
    {
        RPG.EventDispatcher.Register<EnemyKilledEvent>(this, OnEnemyKilled);
    }

    public override void End()
    {
        RPG.EventDispatcher.Unregister<EnemyKilledEvent>(this);
    }

    private void OnEnemyKilled(EnemyKilledEvent evt)
    {
        if (evt.enemy == null || !Data.EnemyType.Type.IsAssignableFrom(evt.enemy.GetType()))
        {
            return;
        }

        currentKillCount++;
        NotifyStepChanged();
    }

    public override bool IsAchieved()
    {
        return currentKillCount >= Data.RequiredKillCount;
    }

    public override float GetProgress()
    {
        return (float)currentKillCount / Data.RequiredKillCount;
    }

    public override string GetProgressText(string format)
    {
        return string.Format(format, Data.Description, currentKillCount, Data.RequiredKillCount);
    }

    public override string Capture()
    {
        return currentKillCount.ToString();
    }

    public override void Restore(string data)
    {
        currentKillCount = int.Parse(data);
    }
}

public class DeliverItemToQuestStep : QuestStep
{
    public new DeliverItemToQuestStepData Data => base.Data as DeliverItemToQuestStepData;

    private InventorySystem inventory;

    private int currentItemCount;

    public DeliverItemToQuestStep(DeliverItemToQuestStepData data)
        : base(data)
    {
        currentItemCount = 0;
    }

    public override void Begin()
    {
        inventory = RPG.LocalPlayer.InventorySystem;

        currentItemCount = inventory.GetTotalItemCount(Data.Item);
        inventory.OnInventoryChanged += OnInventoryChanged;
    }

    public override void End()
    {
        inventory.OnInventoryChanged -= OnInventoryChanged;
    }

    public override void Commit()
    {
        base.Commit();

        var items = inventory.GetItems(Data.Item, Data.Quantity);
        inventory.RemoveItems(items);
    }

    private void OnInventoryChanged()
    {
        currentItemCount = inventory.GetTotalItemCount(Data.Item);
        NotifyStepChanged();
    }

    public override bool IsAchieved()
    {
        return currentItemCount >= Data.Quantity;
    }

    public override float GetProgress()
    {
        return (float)currentItemCount / Data.Quantity;
    }

    public override string GetProgressText(string format)
    {
        return string.Format(format, Data.Description, currentItemCount, Data.Quantity);
    }

    public override UUID GetObjectiveTarget() => Data.TargetID;
}
