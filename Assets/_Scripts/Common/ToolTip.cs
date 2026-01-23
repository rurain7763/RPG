using System.Collections;
using UnityEngine;

public class ToolTip : MonoBehaviour
{
    enum Anchor
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center
    }

    [SerializeField] private Anchor anchor = Anchor.Center;

    private Canvas canvas;
    private RectTransform canvasRectTransform;
    private RectTransform rectTransform;

    private Coroutine showCoroutine;

    protected virtual void Awake()
    {
        canvas = Helper.GetComponentInHighestAncestor<Canvas>(gameObject);
        canvasRectTransform = canvas.GetComponent<RectTransform>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void UpdatePosition(Vector2 targetPosition)
    {
        Vector2 newPosition = targetPosition + GetOffsetByAnchor();

        Vector2 rectSize = rectTransform.sizeDelta;
        Vector2 halfParentSize = canvasRectTransform.sizeDelta / 2.0f;
        Vector2 pivot = rectTransform.pivot;

        float toolTipLeftEdge = newPosition.x - (rectSize.x * pivot.x);
        float toolTipRightEdge = newPosition.x + (rectSize.x * (1 - pivot.x));
        float canvasLeftEdge = -halfParentSize.x;
        float canvasRightEdge = halfParentSize.x;

        if (toolTipLeftEdge < canvasLeftEdge)
        {
            newPosition.x += canvasLeftEdge - toolTipLeftEdge;
        }
        else if (toolTipRightEdge > canvasRightEdge)
        {
            newPosition.x -= toolTipRightEdge - canvasRightEdge;
        }

        float toolTipBottomEdge = newPosition.y - (rectSize.y * pivot.y);
        float toolTipTopEdge = newPosition.y + (rectSize.y * (1 - pivot.y));
        float canvasBottomEdge = -halfParentSize.y;
        float canvasTopEdge = halfParentSize.y;

        if (toolTipBottomEdge < canvasBottomEdge)
        {
            newPosition.y += canvasBottomEdge - toolTipBottomEdge;
        }
        else if (toolTipTopEdge > canvasTopEdge)
        {
            newPosition.y -= toolTipTopEdge - canvasTopEdge;
        }

        rectTransform.anchoredPosition = newPosition;
    }

    public void Show(Vector2 targetPosition)
    {
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        UpdatePosition(targetPosition);
    }

    public void ShowOnPointer(Vector2 screenPoint)
    {
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        Vector3 world;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRectTransform,
            screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out world
        );

        Vector2 local = canvasRectTransform.InverseTransformPoint(world);

        UpdatePosition(local);
    }

    public void Show(RectTransform targetRect)
    {
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }

        showCoroutine = StartCoroutine(ShowCo(targetRect));
    }

    private IEnumerator ShowCo(RectTransform targetRect)
    {
        yield return new WaitForEndOfFrame();

        if (targetRect == null)
        {
            showCoroutine = null;
            yield break;
        }

        Vector2 targetPivot = targetRect.pivot;
        Vector2 targetSize = targetRect.rect.size;
        Vector2 offsetToCenter = new Vector2((0.5f - targetPivot.x) * targetSize.x, (0.5f - targetPivot.y) * targetSize.y);

        Vector2 targetLocalPosition = canvasRectTransform.InverseTransformPoint(targetRect.position);
        targetLocalPosition += offsetToCenter;

        UpdatePosition(targetLocalPosition);

        showCoroutine = null;
    }

    private Vector2 GetOffsetByAnchor()
    {
        Vector2 offset = Vector2.zero;
        Vector2 size = rectTransform.sizeDelta;

        switch (anchor)
        {
            case Anchor.TopLeft:
                offset = new Vector2(size.x / 2, -size.y / 2);
                break;
            case Anchor.TopRight:
                offset = new Vector2(-size.x / 2, -size.y / 2);
                break;
            case Anchor.BottomLeft:
                offset = new Vector2(size.x / 2, size.y / 2);
                break;
            case Anchor.BottomRight:
                offset = new Vector2(-size.x / 2, size.y / 2);
                break;
            case Anchor.Center:
                offset = Vector2.zero;
                break;
        }

        return offset;
    }

    public void Hide()
    {
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        rectTransform.anchoredPosition = new Vector2(10000, 10000);
    }
}