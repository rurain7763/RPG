using TMPro;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    [SerializeField, Reference("FillArea")] private RectTransform fillAreaTransform;
    [SerializeField, Reference("FillArea/HpFill")] private RectTransform hpFillTransform;
    [SerializeField, Reference("FillArea/BarrierFill")] private RectTransform barrierFillTransform;
    [SerializeField, Reference("Text_Hp")] private TMP_Text hpText;

    private float maxHp;
    private float hp;
    private float barrier;

    public void SetHp(float hp, float maxHp)
    {
        this.maxHp = maxHp;
        this.hp = hp;
        UpdateFillImages();
        UpdateTexts();
    }

    public void SetBarrier(float barrier)
    {
        this.barrier = barrier;
        UpdateFillImages();
        UpdateTexts();
    }

    private void UpdateFillImages()
    {
        float fillAreaWidth = fillAreaTransform.rect.width;

        float total = hp + barrier;
        float totalRatio = total / maxHp;

        if (totalRatio > 1.0)
        {
            float hpRatio = hp / total;
            float barrierRatio = barrier / total;

            hpFillTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillAreaWidth * hpRatio);
            barrierFillTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillAreaWidth * barrierRatio);
        }
        else
        {
            float hpRatio = hp / maxHp;
            float barrierRatio = barrier / maxHp;

            hpFillTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillAreaWidth * hpRatio);
            barrierFillTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillAreaWidth * barrierRatio);
        }
    }

    private void UpdateTexts()
    {
        if (hpText != null)
        {
            hpText.text = $"{hp:0.#} / {maxHp:0.#}";
        }
    }

    [ContextMenu("Test Update Fill Images")]
    private void TestUpdateFillImages()
    {
        float randomMaxHp = Random.Range(50f, 200f);
        float randomHp = Random.Range(0f, randomMaxHp);
        float randomBarrier = Random.Range(0f, randomMaxHp - randomHp);

        SetHp(randomHp, randomMaxHp);
        SetBarrier(randomBarrier);
    }
}
