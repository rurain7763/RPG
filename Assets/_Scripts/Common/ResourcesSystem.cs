using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesSystem : AppSystem
{
    private Dictionary<string, AudioClip> audioCache = new();
    private Dictionary<string, Sprite> spriteCache = new();
    private Dictionary<string, GameObject> prefabCache = new();
    private Dictionary<string, ScriptableObject> scriptableObjectCache = new();

    public override void OnAttach(AppManager appManager)
    {
        base.OnAttach(appManager);

        var settings = Instantiate(Resources.Load<ResourcesSystemSetting>("ResourcesSystemSetting"));

        LoadAllAudios(settings);
        LoadAllSprites(settings);
        LoadAllPrefab(settings);
        LoadAllScriptableObject(settings);
    }

    void LoadAllAudios(ResourcesSystemSetting settings)
    {
        foreach (var clip in Resources.LoadAll<AudioClip>(settings.audioBasePath))
        {
            if (!audioCache.ContainsKey(clip.name))
            {
                audioCache.Add(clip.name, clip);
            }
        }
    }

    void LoadAllSprites(ResourcesSystemSetting settings)
    {
        foreach (var tex2D in Resources.LoadAll<Texture2D>(settings.textureBasePath))
        {
            if (!spriteCache.ContainsKey(tex2D.name))
            {
                Sprite sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0.5f, 0.5f));
                spriteCache.Add(tex2D.name, sprite);
            }
        }
    }

    void LoadAllPrefab(ResourcesSystemSetting settings)
    {
        foreach (var prefab in Resources.LoadAll<GameObject>(settings.prefabBasePath))
        {
            if (!prefabCache.ContainsKey(prefab.name))
            {
                prefabCache.Add(prefab.name, prefab);
            }
        }
    }

    private void LoadAllScriptableObject(ResourcesSystemSetting settings)
    {
        foreach (var so in Resources.LoadAll<ScriptableObject>(settings.scriptableObjectBasePath))
        {
            if (!scriptableObjectCache.ContainsKey(so.name))
            {
                scriptableObjectCache.Add(so.name, so);
            }
        }
    }

    public AudioClip GetAudioClip(string name)
    {
        if (audioCache.TryGetValue(name, out var clip))
        {
            return clip;
        }
        Debug.LogWarning($"AudioClip with name {name} not found in cache.");
        return null;
    }

    public bool TryGetAudioClip(string name, out AudioClip clip)
    {
        return audioCache.TryGetValue(name, out clip);
    }

    public Sprite GetSprite(string name)
    {
        if (spriteCache.TryGetValue(name, out var sprite))
        {
            return sprite;
        }
        Debug.LogWarning($"Sprite with name {name} not found in cache.");
        return null;
    }

    public bool TryGetSprite(string name, out Sprite sprite)
    {
        return spriteCache.TryGetValue(name, out sprite);
    }

    public GameObject GetPrefab(string name)
    {
        if (prefabCache.TryGetValue(name, out var prefab))
        {
            return prefab;
        }
        Debug.LogWarning($"Prefab with name {name} not found in cache.");
        return null;
    }

    public bool TryGetPrefab(string name, out GameObject prefab)
    {
        return prefabCache.TryGetValue(name, out prefab);
    }

    public bool TryGetScriptableObject<T>(string name, out T scriptableObject) where T : ScriptableObject
    {
        scriptableObject = null;
        if (scriptableObjectCache.TryGetValue(name, out var so))
        {
            scriptableObject = so as T;
        }

        return so != null;
    }

    public void EachAllScriptableObject<T>(Action<T> action) where T : ScriptableObject
    {
        foreach (var so in scriptableObjectCache.Values)
        {
            if (so is T typedSO)
            {
                action(typedSO);
            }
        }
    }

    public bool TryGetResource<T>(string name, out T resource) where T : UnityEngine.Object
    {
        resource = Resources.Load<T>(name);
        return resource != null;
    }

    public void EachAllResources<T>(string rootPath, Action<T> action) where T : UnityEngine.Object
    {
        foreach (var res in Resources.LoadAll<T>(rootPath))
        {
            action(res);
        }
    }
}