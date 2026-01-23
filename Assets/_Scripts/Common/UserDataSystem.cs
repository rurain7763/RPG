using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

public abstract class UserDataTable
{
    public ulong UUID { get; set; } = 0;
    public ulong Version { get; protected set; } = 0;

    public virtual void Update() { }
    public virtual void Upload() { }
   
    public virtual async UniTask UpdateAsync()
    {
        Update();
        await UniTask.Yield();
    }

    public virtual async UniTask UploadAsync()
    {
        Upload();
        await UniTask.Yield();
    }

    public virtual void Clear() { }
}

public class UserDataSystem : AppSystem
{
    public ulong CurrentUUID { get; protected set; } = 0;

    private Dictionary<ulong, Dictionary<Type, UserDataTable>> tables = new();

    public T AddTable<T>(params object[] args) where T : UserDataTable
    {
        return AddTable<T>(CurrentUUID, args);
    }

    public T AddTable<T>(ulong uuid, params object[] args) where T : UserDataTable
    {
        if (!tables.TryGetValue(uuid, out var userTables))
        {
            userTables = new Dictionary<Type, UserDataTable>();
            tables[uuid] = userTables;
        }

        Type type = typeof(T);
        if (userTables.TryGetValue(type, out var existingTable))
        {
            return existingTable as T;
        }

        T table = Activator.CreateInstance(typeof(T), args) as T;
        table.UUID = uuid;
        userTables[type] = table;

        return table;
    }
    public void RemoveTable<T>() where T : UserDataTable
    {
        RemoveTable<T>(CurrentUUID);
    }

    public void RemoveTable<T>(ulong uuid) where T : UserDataTable
    {
        if (!tables.TryGetValue(uuid, out var userTables))
        {
            return;
        }

        Type type = typeof(T);
        if (userTables.TryGetValue(type, out var table))
        {
            table.Clear();
            userTables.Remove(type);
        }
    }

    public T GetTable<T>() where T : UserDataTable
    {
        return GetTable<T>(CurrentUUID);
    }

    public T GetTable<T>(ulong user) where T : UserDataTable
    {
        return tables[user][typeof(T)] as T;
    }

    public bool TryGetTable<T>(out T table) where T : UserDataTable
    {
        return TryGetTable(CurrentUUID, out table);
    }

    public bool TryGetTable<T>(ulong uuid, out T table) where T : UserDataTable
    {
        table = null;

        if (!tables.TryGetValue(uuid, out var userTables))
        {
            return false;
        }

        Type type = typeof(T);
        if (userTables.TryGetValue(type, out var foundTable))
        {
            table = foundTable as T;
            return true;
        }

        return false;
    }

    public void UpdateTables<T>() where T : UserDataTable
    {
        UpdateTables(CurrentUUID, typeof(T));
    }

    public void UpdateTables<T1, T2>() where T1 : UserDataTable where T2 : UserDataTable
    {
        UpdateTables(CurrentUUID, typeof(T1), typeof(T2));
    }

    public void UpdateTables<T1, T2, T3>() where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable
    {
        UpdateTables(CurrentUUID, typeof(T1), typeof(T2), typeof(T3));
    }

    public void UpdateTables<T1, T2, T3, T4>() where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable
    {
        UpdateTables(CurrentUUID, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    }

    public void UpdateTables<T1, T2, T3, T4, T5>() where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable where T5 : UserDataTable
    {
        UpdateTables(CurrentUUID, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    }

    public void UpdateTables<T1, T2, T3, T4, T5, T6>() where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable where T5 : UserDataTable where T6 : UserDataTable
    {
        UpdateTables(CurrentUUID, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    }

    public void UpdateTables(ulong uuid, params Type[] types)
    {
        if (!tables.TryGetValue(uuid, out var userTables))
        {
            return;
        }

        for (int i = 0; i < types.Length; i++)
        {
            if (!userTables.TryGetValue(types[i], out var table))
            {
                continue;
            }

            table.Update();
        }
    }

    public async UniTask UpdateTablesAsync<T1>(CancellationToken ct = default) where T1 : UserDataTable
    {
        await UpdateTablesAsync(CurrentUUID, ct, typeof(T1));
    }

    public async UniTask UpdateTablesAsync<T1>(ulong uuid, CancellationToken ct = default) where T1 : UserDataTable
    {
        await UpdateTablesAsync(uuid, ct, typeof(T1));
    }

    public async UniTask UpdateTablesAsync<T1, T2>(CancellationToken ct = default) where T1 : UserDataTable where T2 : UserDataTable
    {
        await UpdateTablesAsync(CurrentUUID, ct, typeof(T1), typeof(T2));
    }

    public async UniTask UpdateTablesAsync<T1, T2>(ulong uuid, CancellationToken ct = default) where T1 : UserDataTable where T2 : UserDataTable
    {
        await UpdateTablesAsync(uuid, ct, typeof(T1), typeof(T2));
    }

    public async UniTask UpdateTablesAsync<T1, T2, T3>(CancellationToken ct = default) where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable
    {
        await UpdateTablesAsync(CurrentUUID, ct, typeof(T1), typeof(T2), typeof(T3));
    }

    public async UniTask UpdateTablesAsync<T1, T2, T3>(ulong uuid, CancellationToken ct = default) where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable
    {
        await UpdateTablesAsync(uuid, ct, typeof(T1), typeof(T2), typeof(T3));
    }

    public async UniTask UpdateTablesAsync<T1, T2, T3, T4>(CancellationToken ct = default) where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable
    {
        await UpdateTablesAsync(CurrentUUID, ct, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    }

    public async UniTask UpdateTablesAsync<T1, T2, T3, T4>(ulong uuid, CancellationToken ct = default) where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable
    {
        await UpdateTablesAsync(uuid, ct, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    }

    public async UniTask UpdateTablesAsync<T1, T2, T3, T4, T5>(CancellationToken ct = default) where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable where T5 : UserDataTable
    {
        await UpdateTablesAsync(CurrentUUID, ct, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    }

    public async UniTask UpdateTablesAsync<T1, T2, T3, T4, T5>(ulong uuid, CancellationToken ct = default) where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable where T5 : UserDataTable
    {
        await UpdateTablesAsync(uuid, ct, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    }

    public async UniTask UpdateTablesAsync<T1, T2, T3, T4, T5, T6>(CancellationToken ct = default) where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable where T5 : UserDataTable where T6 : UserDataTable
    {
        await UpdateTablesAsync(CurrentUUID, ct, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    }

    public async UniTask UpdateTablesAsync<T1, T2, T3, T4, T5, T6>(ulong uuid, CancellationToken ct = default) where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable where T5 : UserDataTable where T6 : UserDataTable
    {
        await UpdateTablesAsync(uuid, ct, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    }

    public async UniTask UpdateTablesAsync(ulong uuid, CancellationToken ct, params Type[] types)
    {
        if (!tables.TryGetValue(uuid, out var userTables))
        {
            return;
        }

        List<UniTask> tasks = new();
        for (int i = 0; i < types.Length; i++)
        {
            if (!userTables.TryGetValue(types[i], out var table))
            {
                continue;
            }

            tasks.Add(HandleUpdateTable(table));
        }

        await UniTask.WhenAll(tasks).AttachExternalCancellation(ct);
    }

    private async UniTask HandleUpdateTable(UserDataTable table)
    {
        try
        {
            await table.UpdateAsync();
        }
        catch (Exception ex)
        {
            Logger.Error($"UserDataSystem::HandleUpdateTable Exception: {ex}");
        }
    }

    public void UploadTables<T>() where T : UserDataTable
    {
        UploadTables(typeof(T));
    }

    public void UploadTables<T1, T2>() where T1 : UserDataTable where T2 : UserDataTable
    {
        UploadTables(typeof(T1), typeof(T2));
    }

    public void UploadTables<T1, T2, T3>() where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable
    {
        UploadTables(typeof(T1), typeof(T2), typeof(T3));
    }

    public void UploadTables<T1, T2, T3, T4>() where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable
    {
        UploadTables(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    }

    public void UploadTables<T1, T2, T3, T4, T5>() where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable where T5 : UserDataTable
    {
        UploadTables(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    }

    public void UploadTables<T1, T2, T3, T4, T5, T6>() where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable where T5 : UserDataTable where T6 : UserDataTable
    {
        UploadTables(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    }

    public void UploadTables(params Type[] types)
    {
        if (!tables.TryGetValue(CurrentUUID, out var userTables))
        {
            return;
        }

        for (int i = 0; i < types.Length; i++)
        {
            if (!userTables.TryGetValue(types[i], out var table))
            {
                continue;
            }

            table.Upload();
        }
    }

    public async UniTask UploadTablesAsync<T1>(CancellationToken ct = default) where T1 : UserDataTable
    {
        await UploadTablesAsync(ct, typeof(T1));
    }

    public async UniTask UploadTablesAsync<T1, T2>(CancellationToken ct = default) where T1 : UserDataTable where T2 : UserDataTable
    {
        await UploadTablesAsync(ct, typeof(T1), typeof(T2));
    }

    public async UniTask UploadTablesAsync<T1, T2, T3>(CancellationToken ct = default) where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable
    {
        await UploadTablesAsync(ct, typeof(T1), typeof(T2), typeof(T3));
    }

    public async UniTask UploadTablesAsync<T1, T2, T3, T4>(CancellationToken ct = default) where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable
    {
        await UploadTablesAsync(ct, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    }

    public async UniTask UploadTablesAsync<T1, T2, T3, T4, T5>(CancellationToken ct = default) where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable where T5 : UserDataTable
    {
        await UploadTablesAsync(ct, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    }

    public async UniTask UploadTablesAsync<T1, T2, T3, T4, T5, T6>(CancellationToken ct = default) where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable where T5 : UserDataTable where T6 : UserDataTable
    {
        await UploadTablesAsync(ct, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    }

    public async UniTask UploadTablesAsync(CancellationToken ct, params Type[] types)
    {
        if (!tables.TryGetValue(CurrentUUID, out var userTables))
        {
            return;
        }

        List<UniTask> tasks = new();
        for (int i = 0; i < types.Length; i++)
        {
            if (!userTables.TryGetValue(types[i], out var table))
            {
                continue;
            }

            tasks.Add(HandleUploadTable(table));
        }

        await UniTask.WhenAll(tasks).AttachExternalCancellation(ct);
    }

    private async UniTask HandleUploadTable(UserDataTable table)
    {
        try
        {
            await table.UploadAsync();
        }
        catch (Exception ex)
        {
            Logger.Error($"UserDataSystem::HandleUploadTable Exception: {ex}");
        }
    }

    public void ClearTables<T>() where T : UserDataTable
    {
        ClearTables(typeof(T));
    }

    public void ClearTables<T1, T2>() where T1 : UserDataTable where T2 : UserDataTable
    {
        ClearTables(typeof(T1), typeof(T2));
    }

    public void ClearTables<T1, T2, T3>() where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable
    {
        ClearTables(typeof(T1), typeof(T2), typeof(T3));
    }

    public void ClearTables<T1, T2, T3, T4>() where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable
    {
        ClearTables(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    }

    public void ClearTables<T1, T2, T3, T4, T5>() where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable where T5 : UserDataTable
    {
        ClearTables(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    }

    public void ClearTables<T1, T2, T3, T4, T5, T6>() where T1 : UserDataTable where T2 : UserDataTable where T3 : UserDataTable where T4 : UserDataTable where T5 : UserDataTable where T6 : UserDataTable
    {
        ClearTables(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    }

    public void ClearTables(params Type[] types)
    {
        if (!tables.TryGetValue(CurrentUUID, out var userTables))
        {
            return;
        }

        for (int i = 0; i < types.Length; i++)
        {
            if (!userTables.TryGetValue(types[i], out var table))
            {
                continue;
            }

            table.Clear();
        }
    }
}