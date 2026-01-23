using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public struct SFXExtentionParams
{
    public float minDistance;
    public float maxDistance;
    public bool randomPitch;
    public bool randomStartTime;
    public AudioMixerGroup customMixer;
}

public class AudioSystem : AppSystem
{
    [Serializable]
    struct MixerInfo
    {
        public AudioMixerGroup mixerGroup;
        public string volumeParamName;
    }

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private MixerInfo bgmMixerInfo;
    [SerializeField] private MixerInfo sfxMixerInfo;
    public Transform bgmTransform;
    public Transform sfxTransform;

    private ResourcesSystem resourcesSystem;

    private AudioSource bgmAudioSource = new();

    private List<AudioSource> sfxAudioSources = new();
    private Queue<AudioSource> freeSfxAudioSources = new();

    private Coroutine playBgmCrossFadeCoroutine;

    private bool isSfxMute = false;

    public AudioRolloffMode sfxRolloffMode { get; set; } = AudioRolloffMode.Linear;
    public float sfxMinDistance { get; set; } = 1f;
    public float sfxMaxDistance { get; set; } = 15f;

    public float BGMVolume
    {
        get
        {
            audioMixer.GetFloat(bgmMixerInfo.volumeParamName, out float value);
            return Mathf.Pow(10f, value / 20f);
        }
    }

    public float SFXVolume
    {
        get
        {
            audioMixer.GetFloat(sfxMixerInfo.volumeParamName, out float value);
            return Mathf.Pow(10f, value / 20f);
        }
    }

    public override void OnAttach(AppManager appManager)
    {
        base.OnAttach(appManager);

        resourcesSystem = appManager.GetSystem<ResourcesSystem>();

        GameObject obj = new GameObject($"BGM_0");
        bgmAudioSource = obj.AddComponent<AudioSource>();
        bgmAudioSource.outputAudioMixerGroup = bgmMixerInfo.mixerGroup;
        bgmAudioSource.loop = true;
        bgmAudioSource.playOnAwake = false;
        obj.transform.SetParent(bgmTransform);
    }

    public void PlayBGM(string bgm, float volume = 1.0f)
    {
        if (bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }

        if (!resourcesSystem.TryGetAudioClip(bgm, out AudioClip clip))
        {
            Logger.Warn($"AudioManager: PlayBGM: BGM {bgm} not found.");
            return;
        }

        bgmAudioSource.clip = clip;
        bgmAudioSource.volume = volume;
        bgmAudioSource.Play();
    }

    public void PlayBGMCrossFade(string bgm, float fadeDuration = 1f, float volume = 1.0f)
    {
        if (!resourcesSystem.TryGetAudioClip(bgm, out AudioClip clip))
        {
            Logger.Warn($"AudioManager: PlayBGM: BGM {bgm} not found.");
            return;
        }

        if (playBgmCrossFadeCoroutine != null)
        {
            StopCoroutine(playBgmCrossFadeCoroutine);
        }

        playBgmCrossFadeCoroutine = StartCoroutine(PlayBGMCrossFadeCo(clip, fadeDuration, volume));
    }

    private IEnumerator PlayBGMCrossFadeCo(AudioClip nextClip, float fadeDuration, float volume)
    {
        float currentTime = 0f;

        if (bgmAudioSource.isPlaying)
        {
            float startVolume = bgmAudioSource.volume;
            while (currentTime < fadeDuration)
            {
                currentTime += Time.deltaTime;
                bgmAudioSource.volume = Mathf.Lerp(startVolume, 0, currentTime / fadeDuration);
                yield return null;
            }
            bgmAudioSource.Stop();
        }

        bgmAudioSource.clip = nextClip;
        bgmAudioSource.volume = 0;
        bgmAudioSource.Play();

        currentTime = 0f;

        float targetVolume = volume;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            bgmAudioSource.volume = Mathf.Lerp(0, targetVolume, currentTime / fadeDuration);
            yield return null;
        }

        playBgmCrossFadeCoroutine = null;
    }

    public void PauseBGM()
    {
        if (bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (!bgmAudioSource.isPlaying && bgmAudioSource.clip != null)
        {
            bgmAudioSource.UnPause();
        }
    }

    public void StopBGM()
    {
        if (bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }
    }

    private AudioSource GetFreeAudioSource()
    {
        if (freeSfxAudioSources.Count == 0)
        {
            GameObject obj = new GameObject($"SFX_{sfxAudioSources.Count}");
            AudioSource newAudioSrc = obj.AddComponent<AudioSource>();
            obj.transform.SetParent(sfxTransform);
            sfxAudioSources.Add(newAudioSrc);
            freeSfxAudioSources.Enqueue(newAudioSrc);
        }

        var freeAS = freeSfxAudioSources.Dequeue();
        freeAS.transform.position = Vector3.zero;
        freeAS.outputAudioMixerGroup = sfxMixerInfo.mixerGroup;
        freeAS.mute = isSfxMute;
        freeAS.loop = false;
        freeAS.playOnAwake = false;
        freeAS.pitch = 1f;

        return freeAS;
    }

    private void HandleSFXExtensionParams(AudioSource audioSource, SFXExtentionParams extentionParams)
    {
        audioSource.minDistance = extentionParams.minDistance > 0 ? extentionParams.minDistance : sfxMinDistance;
        audioSource.maxDistance = extentionParams.maxDistance > 0 ? extentionParams.maxDistance : sfxMaxDistance;
        if (extentionParams.randomPitch)
        {
            audioSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        }
        if (extentionParams.randomStartTime)
        {
            audioSource.time = UnityEngine.Random.Range(0f, audioSource.clip.length);
        }
        if (extentionParams.customMixer != null)
        {
            audioSource.outputAudioMixerGroup = extentionParams.customMixer;
        }
    }

    public void PlaySFX(string sfx, float volume = 1.0f, SFXExtentionParams? sFXExtentionParams = null)
    {
        if (!resourcesSystem.TryGetAudioClip(sfx, out AudioClip clip))
        {
            Logger.Warn($"AudioManager: PlaySFX: SFX {sfx} not found.");
            return;
        }

        AudioSource audioSrc = GetFreeAudioSource();
        audioSrc.spatialBlend = 0f;
        audioSrc.clip = clip;
        audioSrc.volume = volume;

        if (sFXExtentionParams.HasValue)
        {
            HandleSFXExtensionParams(audioSrc, sFXExtentionParams.Value);
        }

        StartCoroutine(PlaySFXCo(audioSrc));
    }

    public void PlaySFX(string sfx, Vector3 position, float volume = 1.0f, SFXExtentionParams? sFXExtentionParams = null)
    {
        if (!resourcesSystem.TryGetAudioClip(sfx, out AudioClip clip))
        {
            Logger.Warn($"AudioManager: PlaySFX: SFX {sfx} not found.");
            return;
        }

        AudioSource audioSrc = GetFreeAudioSource();
        audioSrc.spatialBlend = 1f;
        audioSrc.rolloffMode = sfxRolloffMode;
        audioSrc.minDistance = sfxMinDistance;
        audioSrc.maxDistance = sfxMaxDistance;
        audioSrc.transform.position = position;
        audioSrc.clip = clip;
        audioSrc.volume = volume;

        if (sFXExtentionParams.HasValue)
        {
            HandleSFXExtensionParams(audioSrc, sFXExtentionParams.Value);
        }

        StartCoroutine(PlaySFXCo(audioSrc));
    }

    IEnumerator PlaySFXCo(AudioSource audioSource)
    {
        audioSource.Play();
        float remainingTime = audioSource.clip.length - audioSource.time;
        yield return new WaitForSecondsRealtime(remainingTime);
        freeSfxAudioSources.Enqueue(audioSource);
    }

    public AudioSource PlayLoopSFX(string sfx, float volume = 1.0f, SFXExtentionParams? sFXExtentionParams = null)
    {
        if (!resourcesSystem.TryGetAudioClip(sfx, out AudioClip clip))
        {
            Logger.Warn($"AudioManager: PlayLoopSFX: SFX {sfx} not found.");
            return null;
        }
        AudioSource audioSrc = GetFreeAudioSource();
        audioSrc.spatialBlend = 0f;
        audioSrc.clip = clip;
        audioSrc.volume = volume;
        audioSrc.loop = true;

        if (sFXExtentionParams.HasValue)
        {
            HandleSFXExtensionParams(audioSrc, sFXExtentionParams.Value);
        }

        audioSrc.Play();
        return audioSrc;
    }

    public AudioSource PlayLoopSFX(string sfx, Vector3 position, float volume = 1.0f, SFXExtentionParams? sFXExtentionParams = null)
    {
        if (!resourcesSystem.TryGetAudioClip(sfx, out AudioClip clip))
        {
            Logger.Warn($"AudioManager: PlayLoopSFX: SFX {sfx} not found.");
            return null;
        }

        AudioSource audioSrc = GetFreeAudioSource();
        audioSrc.spatialBlend = 1f;
        audioSrc.rolloffMode = sfxRolloffMode;
        audioSrc.minDistance = sfxMinDistance;
        audioSrc.maxDistance = sfxMaxDistance;
        audioSrc.transform.position = position;
        audioSrc.clip = clip;
        audioSrc.volume = volume;
        audioSrc.loop = true;

        if (sFXExtentionParams.HasValue)
        {
            HandleSFXExtensionParams(audioSrc, sFXExtentionParams.Value);
        }

        audioSrc.Play();

        return audioSrc;
    }

    public void StopLoopSFX(AudioSource audioSource)
    {
        if (audioSource == null)
        {
            return;
        }

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.loop = false;
        freeSfxAudioSources.Enqueue(audioSource);
    }

    public void SetMuteBGM(bool isMute)
    {
        bgmAudioSource.mute = isMute;
    }

    public void SetMuteSFX(bool isMute)
    {
        foreach (var audioSrc in sfxAudioSources)
        {
            audioSrc.mute = isMute;
        }

        isSfxMute = isMute;
    }

    public void SetMute(bool isMute)
    {
        SetMuteBGM(isMute);
        SetMuteSFX(isMute);
    }

    private float LinearToDecibel(float linear)
    {
        linear = Mathf.Max(Mathf.Epsilon, linear);
        return 20f * Mathf.Log10(linear);
    }

    public void SetBGMVolume(float volume)
    {
        volume = Mathf.Max(Mathf.Epsilon, volume);
        audioMixer.SetFloat(bgmMixerInfo.volumeParamName, LinearToDecibel(volume));
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Max(Mathf.Epsilon, volume);
        audioMixer.SetFloat(sfxMixerInfo.volumeParamName, LinearToDecibel(volume));
    }

    public void SetVolume(float volume)
    {
        SetBGMVolume(volume);
        SetSFXVolume(volume);
    }

    public override Type[] GetDependencySystemTypes()
    {
        return new Type[] { typeof(ResourcesSystem) };
    }
}