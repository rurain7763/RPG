using UnityEngine;

public class WorldToUIFollower : MonoBehaviour
{
    private Canvas canvas;
    private Transform anchor;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
    }

    private void LateUpdate()
    {
        if (canvas == null || anchor == null)
        {
            return;
        }

        Vector2 screenPoint = anchor.position;
        if (canvas.worldCamera != null)
        {
            screenPoint = canvas.worldCamera.WorldToScreenPoint(anchor.position);
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, screenPoint, canvas.worldCamera, out var localPoint);
        rectTransform.anchoredPosition = localPoint;
    }

    public void SetAnchor(Transform anchor)
    {
        canvas = Helper.GetComponentInHighestAncestor<Canvas>(gameObject);
        this.anchor = anchor;
    }
}