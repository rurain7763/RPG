using System;
using System.Collections.Generic;
using UnityEngine;

public class VFXSystem : AppSystem
{
    private List<VFX> vfxs = new();
    private Dictionary<string, PooledObjectPool> reusableVfxs = new();

    ResourcesSystem resourcesSystem;

    public override void OnAttach(AppManager appManager)
    {
        base.OnAttach(appManager);

        resourcesSystem = appManager.GetSystem<ResourcesSystem>();
    }

    public VFX SpawnVFX(string effectName, Transform anchor = null)
    {
        if (TryGetReusableVFX(effectName, out var reusablePool, out var reusableVfx, out var reusablePooledVfx))
        {
            reusableVfx.Init(() =>
            {
                vfxs.Remove(reusableVfx);
                reusablePool.ReleaseObject(reusablePooledVfx);
            });

            vfxs.Add(reusableVfx);

            return reusableVfx;
        }

        var effectPrefab = resourcesSystem.GetPrefab(effectName);
        if (effectPrefab == null)
        {
            Logger.Error($"EffectSystem: Effect prefab '{effectName}' not found.");
            return null;
        }

        VFX vfx = null;

        if (effectPrefab.TryGetComponent<PooledObject>(out var prefabComponent))
        {
            var pool = GetPoolOrCreateIfNeeded(effectName, prefabComponent);
            var pooledObject = pool.GetObject();
            vfx = pooledObject.GetComponent<VFX>();
            if (vfx == null)
            {
                Logger.Error($"EffectSystem: VFX component not found on pooled object for '{effectName}'.");
                return null;
            }

            vfx.Init(() =>
            {
                vfxs.Remove(vfx);
                pool.ReleaseObject(pooledObject);
            });
        }
        else
        {
            var effectGo = Instantiate(effectPrefab, anchor);
            vfx = effectGo.GetComponent<VFX>();
            if (vfx == null)
            {
                Logger.Error($"EffectSystem: Effect component not found on prefab '{effectName}'.");
                Destroy(effectGo);
                return null;
            }

            vfx.Init(() =>
            {
                vfxs.Remove(vfx);
                Destroy(vfx.gameObject);
            });
        }

        vfxs.Add(vfx);

        return vfx;
    }

    private bool TryGetReusableVFX(string effectName, out PooledObjectPool pool, out VFX vfx, out PooledObject pooledObj)
    {
        vfx = null;
        pooledObj = null;
        if (!reusableVfxs.TryGetValue(effectName, out pool))
        {
            return false;
        }

        var pooledObject = pool.GetObject();
        vfx = pooledObject.GetComponent<VFX>();
        pooledObj = pooledObject;

        return vfx != null;
    }

    private PooledObjectPool GetPoolOrCreateIfNeeded(string effectName, PooledObject prefabComponent)
    {
        if (reusableVfxs.ContainsKey(effectName))
        {
            return reusableVfxs[effectName];
        }

        var pool = gameObject.AddComponent<PooledObjectPool>();
        pool.PoolName = $"{effectName}VFXPool";
        pool.Prefab = prefabComponent;
        pool.InitialSize = 1;
        pool.DynamicGrowth = true;
        pool.Initialize();

        reusableVfxs[effectName] = pool;

        return pool;
    }

    public void DestroyVFX(VFX vfx)
    {
        if (!vfxs.Contains(vfx))
        {
            return;
        }

        vfx.DestroySelf();
    }

    public override Type[] GetDependencySystemTypes()
    {
        return new Type[]
        {
            typeof(ResourcesSystem)
        };
    }
}