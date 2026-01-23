using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class AppDataTable
{
    public ulong Version { get; protected set; } = 0;

    public virtual void Update() { }
    public virtual async UniTask UpdateAsync()
    {
        Update();
        await UniTask.Yield();
    }
}

public class AppDataSystem : AppSystem
{
    private Dictionary<Type, AppDataTable> tables = new();

    public T AddTable<T>(params object[] args) where T : AppDataTable
    {
        Type type = typeof(T);

        if (tables.TryGetValue(type, out var existingTable))
        {
            return existingTable as T;
        }

        T table = Activator.CreateInstance(typeof(T), args) as T;
        tables[type] = table;

        return table;
    }

    public void RemoveTable<T>() where T : AppDataTable
    {
        Type type = typeof(T);
        if (tables.TryGetValue(type, out var table))
        {
            tables.Remove(type);
        }
    }

    public T GetTable<T>() where T : AppDataTable
    {
        Type type = typeof(T);
        if (tables.TryGetValue(type, out var table))
        {
            return table as T;
        }
        return null;
    }

    public void UpdateTables<T1>() where T1 : AppDataTable
    {
        UpdateTables(typeof(T1));
    }

    public void UpdateTables<T1, T2>() where T1 : AppDataTable where T2 : AppDataTable
    {
        UpdateTables(typeof(T1), typeof(T2));
    }

    public void UpdateTables<T1, T2, T3>() where T1 : AppDataTable where T2 : AppDataTable where T3 : AppDataTable
    {
        UpdateTables(typeof(T1), typeof(T2), typeof(T3));
    }

    public void UpdateTables(params Type[] types)
    {
        for (int i = 0; i < types.Length; i++)
        {
            if (!tables.TryGetValue(types[i], out var table))
            {
                continue;
            }

            table.Update();
        }
    }

    public async UniTask UpdateTablesAsync<T1>(CancellationToken ct) where T1 : AppDataTable
    {
        await UpdateTablesAsync(ct, typeof(T1));
    }

    public async UniTask UpdateTablesAsync<T1, T2>(CancellationToken ct) where T1 : AppDataTable where T2 : AppDataTable
    {
        await UpdateTablesAsync(ct, typeof(T1), typeof(T2));
    }

    public async UniTask UpdateTablesAsync<T1, T2, T3>(CancellationToken ct) where T1 : AppDataTable where T2 : AppDataTable where T3 : AppDataTable
    {
        await UpdateTablesAsync(ct, typeof(T1), typeof(T2), typeof(T3));
    }

    public async UniTask UpdateTablesAsync(CancellationToken ct, params Type[] types)
    {
        List<UniTask> updateTasks = new();
        for (int i = 0; i < types.Length; i++)
        {
            if (!tables.TryGetValue(types[i], out var table))
            {
                continue;
            }

            updateTasks.Add(UpdateTableAsync(table));
        }

        await UniTask.WhenAll(updateTasks).AttachExternalCancellation(ct);
    }

    private async UniTask UpdateTableAsync(AppDataTable table)
    {
        try
        {
            await table.UpdateAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError($"AppDataSystem::UpdateTableAsync Exception: {ex}");
        }
    }
}