using DG.Tweening;
using TMPro;
using UnityEngine;

public enum FloatingTextAnimType
{
    Normal,
    Critical,
    Heal,
}

[RequireComponent(typeof(TextMeshProUGUI))]
public class CombatText : MonoBehaviour
{
    TextMeshProUGUI strText;

    void Awake() => strText = GetComponent<TextMeshProUGUI>();

    public void SetText(string text) => strText.text = text;
    public void SetValue(int value) => strText.text = value.ToString();
    public void SetValue(float value, int decimalCount = 1) => strText.text = value.ToString($"F{decimalCount}");
    public void SetTextColor(Color color) => strText.color = color;

    public void Play(FloatingTextAnimType animType, TweenCallback onComplete = null)
    {
        switch (animType)
        {
            case FloatingTextAnimType.Normal:
                PlayNormal(onComplete);
                break;
            case FloatingTextAnimType.Critical:
                PlayCritical(onComplete);
                break;
            case FloatingTextAnimType.Heal:
                PlayHeal(onComplete);
                break;
        }
    }

    void PlayNormal(TweenCallback onComplate)
    {
        strText.alpha = 1f;
        transform.localScale = Vector3.one;

        var seq = DOTween.Sequence();
        seq.Append(transform.DOScale(1.2f, 0.1f).SetEase(Ease.OutBack));
        seq.Append(transform.DOScale(1f, 0.1f).SetEase(Ease.InBack));
        seq.Join(transform.DOMoveY(transform.position.y + 1f, 0.6f).SetEase(Ease.OutCubic));
        seq.Join(strText.DOFade(0f, 0.6f).SetEase(Ease.InQuad));
        seq.OnComplete(onComplate);
    }

    void PlayCritical(TweenCallback onComplete)
    {
        strText.alpha = 1f;
        transform.localScale = Vector3.one;

        var seq = DOTween.Sequence();
        seq.Append(transform.DOScale(1.3f, 0.2f).SetEase(Ease.OutElastic));
        seq.Join(strText.DOColor(Color.red, 0.1f));
        seq.Append(transform.DOShakePosition(0.3f, 0.2f, 10, 90));
        seq.Append(transform.DOMoveY(transform.position.y + 1.5f, 0.5f).SetEase(Ease.OutCubic));
        seq.Join(strText.DOFade(0f, 0.5f).SetEase(Ease.Linear));
        seq.OnComplete(onComplete);
    }

    void PlayHeal(TweenCallback onComplete)
    {
        strText.alpha = 1f;
        transform.localScale = Vector3.one;
        strText.color = Color.green;

        var seq = DOTween.Sequence();
        seq.Append(transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutQuad));
        seq.Join(transform.DOMoveY(transform.position.y + 0.8f, 1.0f).SetEase(Ease.OutSine));
        seq.Join(strText.DOFade(0f, 1.0f).SetEase(Ease.Linear));
        seq.OnComplete(onComplete);
    }
}
