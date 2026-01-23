using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public UnityEvent<PointerEventData> OnPointerEnterEvent;
    public UnityEvent<PointerEventData> OnPointerMoveEvent;
    public UnityEvent<PointerEventData> OnPointerExitEvent;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterEvent?.Invoke(eventData);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        OnPointerMoveEvent?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitEvent?.Invoke(eventData);
    }
}