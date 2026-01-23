using System;

namespace Experimental
{
    public enum GameplayAbilityInstancePolicy
    {
        InstancedPerOwner,
        InstancedPerExecution
    }

    public abstract class GameplayAbility
    {
        public readonly GameplayAbilityData AbilityData;
        public int Level { get; set; }
        public bool IsActive { get; protected set; }

        internal GameplayAbilitySystem abilitySystem;

        private FastList<GameplayAbilityTask> activeTasks = new();

        public GameplayAbility(GameplayAbilityData data, int level)
        {
            AbilityData = data;
            Level = level;
        }

        public virtual void Commit()
        {
            CommitCost();
            CommitCooldown();
        }

        public void CommitCost()
        {
            if (AbilityData.CostEffect != null)
            {
                abilitySystem.ApplyGameplayEffectToSelf(AbilityData.CostEffect, Level);
            }
        }

        public void CommitCooldown()
        {
            if (AbilityData.CooldownEffect != null)
            {
                abilitySystem.ApplyGameplayEffectToSelf(AbilityData.CooldownEffect, Level);
            }
        }

        public virtual void Activate()
        {
            IsActive = true;
        }

        public virtual void Execute(float delta)
        {
            for (int i = activeTasks.Count - 1; i >= 0; i--)
            {
                var task = activeTasks[i];
                task.Execute(delta);
                if (task.IsComplete())
                {
                    var chainNext = task.GetNext();
                    if (chainNext != null)
                    {
                        AddTask(chainNext);
                    }

                    activeTasks.RemoveAt(i);
                }
            }
        }

        public virtual void Cancel()
        {
            foreach (var task in activeTasks)
            {
                task.Cancel();
            }
            activeTasks.Clear();

            IsActive = false;
        }

        public virtual void End()
        {
            IsActive = false;
        }

        protected void AddTask(GameplayAbilityTask task)
        {
            task.Enter(this);
            activeTasks.Add(task);
        }
    }

    public abstract class GameplayAbilityTask
    {
        protected GameplayAbility ability;
        protected GameplayAbilityTask prev;
        protected GameplayAbilityTask next;

        public abstract bool IsComplete();

        public virtual void Enter(GameplayAbility ability)
        {
            this.ability = ability;
        }

        public virtual void Execute(float delta) { }
        public virtual void Cancel() { }

        public GameplayAbilityTask Then(GameplayAbilityTask nextTask)
        {
            GameplayAbilityTask root = this;
            while (root.prev != null)
            {
                root = root.prev;
            }

            nextTask.prev = this;
            next = nextTask;

            return root;
        }

        public GameplayAbilityTask GetNext()
        {
            return next;
        }
    }

    #region Useful Ability Tasks
    public class WaitDelayTask : GameplayAbilityTask
    {
        private float delay;
        private float elapsedTime;

        public WaitDelayTask(float delay)
        {
            this.delay = delay;
            elapsedTime = 0f;
        }

        public override bool IsComplete()
        {
            return elapsedTime >= delay;
        }

        public override void Execute(float delta)
        {
            elapsedTime += delta;
        }
    }

    public class ActionTask : GameplayAbilityTask
    {
        private Action action;

        private bool isComplete;

        public ActionTask(Action action)
        {
            this.action = action;
            isComplete = false;
        }

        public override bool IsComplete()
        {
            return isComplete;
        }

        public override void Enter(GameplayAbility ability)
        {
            base.Enter(ability);

            action?.Invoke();
            isComplete = true;
        }
    }
    #endregion
}