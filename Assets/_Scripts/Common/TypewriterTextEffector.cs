using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TypewriterTextEffector : MonoBehaviour
{
    [Serializable]
    struct PausePoint
    {
        public string character;
        public float appendSeconds;
    }

    [SerializeField] private float defaultInterval = 0.05f;
    [SerializeField] private bool dynamicInterval = false;
    [SerializeField] private float dynamicIntervalFactor = 1f;
    [SerializeField] private float maxInterval = 0.1f;
    [SerializeField] private PausePoint[] pausePoints;

    private TMP_Text textComponent;

    private readonly Dictionary<char, float> pausePointDict = new(); 
    private Coroutine activeRoutine;

    public bool IsPlaying => activeRoutine != null;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        foreach (var pausePoint in pausePoints)
        {
            pausePointDict[pausePoint.character[0]] = pausePoint.appendSeconds;
        }
    }

    public void Play(Action onComplete = null)
    {
        Play(defaultInterval, onComplete);
    }

    public void Play(float interval, Action onComplete = null)
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        if (dynamicInterval)
        {
            interval = Mathf.Clamp(interval * (dynamicIntervalFactor / textComponent.text.Length), 0.01f, maxInterval);
        }

        activeRoutine = StartCoroutine(PlayCo(interval, onComplete));
    }

    private IEnumerator PlayCo(float interval, Action onComplete)
    {
        textComponent.maxVisibleCharacters = 0;
        textComponent.ForceMeshUpdate();

        int totalCharacters = textComponent.textInfo.characterCount;

        for (int i = 0; i < totalCharacters; i++)
        {
            textComponent.maxVisibleCharacters = i + 1;

            char lastChar = textComponent.textInfo.characterInfo[i].character;

            float waitSecs = interval;
            if (pausePointDict.TryGetValue(lastChar, out float append))
            {
                waitSecs += append;
            }

            yield return new WaitForSeconds(waitSecs);
        }

        onComplete?.Invoke();

        activeRoutine = null;
    }

    public void Stop()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
            textComponent.maxVisibleCharacters = textComponent.textInfo.characterCount;
        }
    }
}