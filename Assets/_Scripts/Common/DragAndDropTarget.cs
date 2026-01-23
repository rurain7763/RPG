using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DragAndDropTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    private bool isPointerOver;

    public UnityEvent<bool> OnPointerEnterExit;
    public Func<IDragAndDropPayload, bool> OnPayloadDrop;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            return;
        }

        isPointerOver = true;
        OnPointerEnterExit?.Invoke(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isPointerOver)
        {
            return;
        }

        isPointerOver = false;
        OnPointerEnterExit?.Invoke(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject pointerDrag = eventData.pointerDrag;
        if (pointerDrag == null)
        {
            return;
        }

        DragAndDropSource dragAndDropSource = pointerDrag.GetComponent<DragAndDropSource>();
        if (dragAndDropSource == null)
        {
            return;
        }

        IDragAndDropPayload payload = dragAndDropSource.GetPayload();
        if (payload != null)
        {
            bool handled = false;
            if (OnPayloadDrop != null)
            {
                handled = OnPayloadDrop.Invoke(payload);
            }

            if (handled)
            {
                dragAndDropSource.MarkPayloadHandled();
            }
        }
    }
}