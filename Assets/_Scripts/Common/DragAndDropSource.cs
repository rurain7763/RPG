using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IDragAndDropPayload
{
}

public class DragAndDropSource : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private MaskableGraphic graphics;

    private bool payloadHandled;
    private IDragAndDropPayload payload;
    private GameObject dragProxyInstance;

    public Action<PointerEventData, GameObject> OnPointerBeginDrag;
    public Action<bool> OnPointerEndDrag;

    private void Awake()
    {
        graphics = GetComponent<MaskableGraphic>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        eventData.pointerDrag = gameObject;

        payloadHandled = false;
        dragProxyInstance = new GameObject("DragProxy", typeof(CanvasGroup), typeof(RectTransform));
        dragProxyInstance.layer = LayerMask.NameToLayer("UI");
        dragProxyInstance.transform.position = eventData.position;

        CanvasGroup group = dragProxyInstance.GetComponent<CanvasGroup>();
        group.blocksRaycasts = false;

        graphics.raycastTarget = false;

        OnPointerBeginDrag?.Invoke(eventData, dragProxyInstance);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragProxyInstance != null)
        {
            dragProxyInstance.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnPointerEndDrag?.Invoke(payloadHandled);

        graphics.raycastTarget = true;

        Destroy(dragProxyInstance);

        payload = null;
    }

    public void MarkPayloadHandled()
    {
        payloadHandled = true;
    }

    public void SetPayload(IDragAndDropPayload payload)
    {
        this.payload = payload;
    }

    public IDragAndDropPayload GetPayload()
    {
        return payload;
    }
}