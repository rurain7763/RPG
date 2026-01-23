using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public struct AsyncLoadLevelOperation
{
    public class Internal
    {
        public float Progress;
        public bool IsDone;
        public bool AllowToNextLevel;
    }

    private readonly Internal opInternal;

    public float Progress => opInternal.Progress;
    public bool IsDone => opInternal.IsDone;

    public bool AllowToNextLevel
    {
        get => opInternal.AllowToNextLevel;
        set => opInternal.AllowToNextLevel = value;
    }

    public AsyncLoadLevelOperation(Internal internalOperation)
    {
        opInternal = internalOperation;
    }
}

public class LevelSystem : AppSystem
{
    [SerializeField] private LevelEnvironment levelEnvironment;
    [SerializeField] private string startLevelName;

    private ResourcesSystem resourcesSystem;

    private Coroutine activeLoadCoroutine;

    private Level currentLevel;

    public LevelEnvironment Environment
    {
        get => levelEnvironment;
        set => levelEnvironment = value;
    }

    public Level CurrentLevel => currentLevel;

    private GameObject GetLevelPrefab(string levelName)
    {
        if (resourcesSystem.TryGetPrefab(levelName, out GameObject prefab))
        {
            if (prefab.GetComponent<Level>() == null)
            {
                throw new InvalidOperationException($"The prefab '{prefab.name}' does not contain a Level component.");
            }
            return prefab;
        }

        throw new InvalidOperationException($"Prefab for level '{levelName}' not found.");
    }

    public override void OnAttach(AppManager appManager)
    {
        base.OnAttach(appManager);

        resourcesSystem = appManager.GetSystem<ResourcesSystem>();

        if (string.IsNullOrEmpty(startLevelName))
        {
            return;
        }

        LoadLevel(startLevelName);
    }

    private void Update()
    {
        currentLevel?.Execute(Time.deltaTime);
    }

    public void LoadLevel<T>() where T : Level
    {
        LoadLevel(typeof(T).Name);
    }

    public void LoadLevel(string levelName)
    {
        if (activeLoadCoroutine != null)
        {
            throw new InvalidOperationException("A level is already being loaded.");
        }

        if (!resourcesSystem.TryGetPrefab(levelName, out GameObject prefab))
        {
            throw new InvalidOperationException($"Prefab for level '{levelName}' not found.");
        }

        activeLoadCoroutine = StartCoroutine(LoadLevelCo(prefab));
    }

    private IEnumerator LoadLevelCo(GameObject prefab)
    {
        GameObject levelObj = Instantiate(prefab, transform);

        Level nextLevel = levelObj.GetComponent<Level>();
        nextLevel.Init(this);

        if (currentLevel != null)
        {
            currentLevel.Exit();
            Destroy(currentLevel.gameObject);
            currentLevel = null;
        }

        yield return null;

        currentLevel = nextLevel;
        currentLevel.Enter();

        activeLoadCoroutine = null;
    }

    public AsyncLoadLevelOperation LoadLevelAsync<T>(CancellationToken? ct = null) where T : Level
    {
        return LoadLevelAsync(typeof(T).Name, ct);
    }

    public AsyncLoadLevelOperation LoadLevelAsync(string levelName, CancellationToken? ct = null)
    {
        if (activeLoadCoroutine != null)
        {
            throw new InvalidOperationException("A level is already being loaded asynchronously.");
        }

        AsyncLoadLevelOperation.Internal opInternal = new()
        {
            Progress = 0f,
            IsDone = false,
            AllowToNextLevel = false
        };

        GameObject prefab = GetLevelPrefab(levelName);

        activeLoadCoroutine = StartCoroutine(LoadLevelAsyncCo(prefab, opInternal, ct));

        return new AsyncLoadLevelOperation(opInternal);
    }

    private IEnumerator LoadLevelAsyncCo(GameObject prefab, AsyncLoadLevelOperation.Internal opInternal, CancellationToken? ct = null)
    {
        var asyncOperation = InstantiateAsync(prefab, 1, transform, Vector3.zero, Quaternion.identity, ct ?? CancellationToken.None);
        asyncOperation.allowSceneActivation = false;
        while (!asyncOperation.isDone)
        {
            opInternal.Progress = asyncOperation.progress;
            if (asyncOperation.progress >= 0.9f)
            {
                break;
            }
            yield return null;
        }

        while (!asyncOperation.allowSceneActivation)
        {
            asyncOperation.allowSceneActivation = opInternal.AllowToNextLevel;
            yield return null;
        }

        while (!asyncOperation.isDone)
        {
            opInternal.Progress = asyncOperation.progress;
            yield return null;
        }

        if (ct?.IsCancellationRequested == true)
        {
            if (asyncOperation.Result != null && asyncOperation.Result.Length > 0)
            {
                foreach (var obj in asyncOperation.Result)
                {
                    if (obj != null)
                    {
                        Destroy(obj.gameObject);
                    }
                }
            }
        }
        else
        {
            opInternal.IsDone = true;
            opInternal.Progress = 1f;
            
            Level nextLevel = asyncOperation.Result[0].GetComponent<Level>();
            nextLevel.Init(this);

            if (currentLevel != null)
            {
                currentLevel.Exit();
                Destroy(currentLevel.gameObject);
                currentLevel = null;
            }

            yield return null;

            currentLevel = nextLevel;
            currentLevel.Enter();
        }

        activeLoadCoroutine = null;
    }

    public override Type[] GetDependencySystemTypes()
    {
        return new Type[]
        {
            typeof(ResourcesSystem)
        };
    }
}
