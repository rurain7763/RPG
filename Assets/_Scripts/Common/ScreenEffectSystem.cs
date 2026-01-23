using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenEffectSystem : AppSystem
{
    public Image fullScreenImg;

    private Coroutine fadeCorutine;

    private void Awake()
    {
        if (fullScreenImg == null)
        {
            Debug.LogError("FullScreenImg is not assigned in ScreenEffectSystem.");
            return;
        }
    }

    public void FadeIn(float duration, Action onComplete = null)
    {
        if (fadeCorutine != null)
        {
            StopCoroutine(fadeCorutine);
        }

        fadeCorutine = StartCoroutine(FadeCoroutine(1f, 0f, duration, onComplete));
    }

    public void FadeOut(float duration, Action onComplete = null)
    {
        if (fadeCorutine != null)
        {
            StopCoroutine(fadeCorutine);
        }
        fadeCorutine = StartCoroutine(FadeCoroutine(0f, 1f, duration, onComplete));
    }

    public void FadeInOut(float duration, Action onComplete = null)
    {
        float halfDuration = duration / 2f;
        FadeIn(halfDuration, () => { FadeOut(halfDuration, onComplete); });
    }

    public void FadeOutIn(float duration, Action onComplete = null)
    {
        float halfDuration = duration / 2f;
        FadeOut(halfDuration, () => { FadeIn(halfDuration, onComplete); });
    }

    private IEnumerator FadeCoroutine(float from, float to, float duration, Action onComplete)
    {
        fullScreenImg.enabled = true;

        float elapsedTime = 0f;
        Color color = fullScreenImg.color;
        color.a = from;
        fullScreenImg.color = color;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(from, to, elapsedTime / duration);
            fullScreenImg.color = color;
            yield return null;
        }
        color.a = to;
        fullScreenImg.color = color;
        fadeCorutine = null;

        if (to <= 0f)
        {
            fullScreenImg.enabled = false;
        }

        onComplete?.Invoke();
    }

    public void Clear()
    {
        if (fadeCorutine != null)
        {
            StopCoroutine(fadeCorutine);
            fadeCorutine = null;
        }

        fullScreenImg.enabled = false;
    }

    private void OnValidate()
    {
        if (fullScreenImg != null)
        {
            fullScreenImg.rectTransform.anchorMin = Vector2.zero;
            fullScreenImg.rectTransform.anchorMax = Vector2.one;
            fullScreenImg.rectTransform.anchoredPosition = Vector2.zero;
            fullScreenImg.rectTransform.sizeDelta = Vector2.zero;
        }
    }
}
