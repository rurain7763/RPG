using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DomainExpansion : RPGSkill
{
    public new DomainExpansionData Data => base.Data as DomainExpansionData;

    private Territory domain;
    private List<Entity> entitiesInDomain = new();
    private Dictionary<Entity, Exhausted> affectedEntities = new();

    private Coroutine shardSpamCoroutine;

    private FastList<Echo> activeEchoes = new();
    private Coroutine echoSpamCoroutine;

    public DomainExpansion(DomainExpansionData data) 
        : base(data)
    {
    }

    protected override void StartUse(GameObject user, Arguments args = null)
    {
        base.StartUse(user, args);

        affectedEntities.Clear();
        entitiesInDomain.Clear();

        domain = Object.Instantiate(Data.TerritoryPrefab, entity.CenterPosition, Quaternion.identity, entity.IncludedLevel.transform);
        domain.LifeTime = Data.ActiveDuration;
        domain.OnEntityEnter += HandleEntityEnter;
        domain.OnEntityExit += HandleEntityExit;
        domain.Expand(Data.TargetSize, Data.ExpansionDuration);

        if (HasUpgrade(DomainExpansionUpgradeFlag.ShardSpam))
        {
            SpawnShardRepeatedly();
        }

        if (HasUpgrade(DomainExpansionUpgradeFlag.EchoSpam))
        {
            SpawnEchoRepeatedly();
        }
    }

    private void HandleEntityEnter(Entity other)
    {
        if (other is not ICombatable combatable)
        {
            return;
        }

        var buff = new Exhausted(Buff.InfiniteDuration, 0.5f, entity as ICombatable);
        combatable.BuffSystem.AddBuff(buff);

        entitiesInDomain.Add(other);
        affectedEntities[other] = buff;
    }

    private void HandleEntityExit(Entity other)
    {
        if (affectedEntities.TryGetValue(other, out var buff))
        {
            var combatable = other as ICombatable;
            combatable.BuffSystem.RemoveBuff(buff);
            affectedEntities.Remove(other);
            entitiesInDomain.Remove(other);
        }
    }

    private Entity GetRandomTargetInDomain()
    {
        int rndIndex = Random.Range(0, entitiesInDomain.Count);
        if (entitiesInDomain.Count > 0)
        {
            return entitiesInDomain[rndIndex];
        }
        return null;
    }

    private void SpawnShardRepeatedly()
    {
        if (shardSpamCoroutine != null)
        {
            entity.StopCoroutine(shardSpamCoroutine);
        }
        shardSpamCoroutine = entity.StartCoroutine(SpawnShardsRepeatedlyCo());
    }

    private IEnumerator SpawnShardsRepeatedlyCo()
    {
        while (domain != null)
        {
            var target = GetRandomTargetInDomain();
            if (target != null)
            {
                var shard = GameObject.Instantiate(Data.ShardPrefab, domain.transform.position, Quaternion.identity, entity.IncludedLevel.transform);
                shard.Owner = entity;
                shard.MoveToManualTarget = true;
                shard.ManualTarget = target;
            }

            yield return new WaitForSeconds(Data.ShardSpamInterval);
        }

        shardSpamCoroutine = null;
    }

    private void SpawnEchoRepeatedly()
    {
        if (echoSpamCoroutine != null)
        {
            entity.StopCoroutine(echoSpamCoroutine);
        }
        echoSpamCoroutine = entity.StartCoroutine(SpawnEchoesRepeatedlyCo());
    }

    private IEnumerator SpawnEchoesRepeatedlyCo()
    {
        while (domain != null)
        {
            var target = GetRandomTargetInDomain();
            if (target != null)
            {
                var spawnPosition = target.transform.position;
                spawnPosition.x += target.IsFacingRight ? -target.CenterToBackDistance : target.CenterToFrontDistance;
                var echo = Object.Instantiate(Data.EchoPrefab, spawnPosition, Quaternion.identity);
                echo.MaxAttackCount = 1;
                echo.LookAt(target.transform.position);
                echo.Begin();
                activeEchoes.Add(echo);
            }
            
            yield return new WaitForSeconds(Data.EchoSpamInterval);
        }

        echoSpamCoroutine = null;
    }

    public override void Tick(float delta)
    {
        base.Tick(delta);

        for (int i = activeEchoes.Count - 1; i >= 0; i--)
        {
            var echo = activeEchoes[i];
            if (echo == null)
            {
                activeEchoes.RemoveAt(i);
            }
        }
    }

    public override bool CanUse(GameObject user)
    {
        return base.CanUse(user) && domain == null;
    }
}
