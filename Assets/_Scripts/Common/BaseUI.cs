using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(GraphicRaycaster))]
public abstract class BaseUI : MonoBehaviour
{
    protected UISystem uiSystem;

    private Canvas canvas;

    public int SortingOrder
    {
        get => canvas.sortingOrder;
        set => canvas.sortingOrder = value;
    }

    public virtual void Init(UISystem system) {
        uiSystem = system;

        canvas = GetComponent<Canvas>();
        if (!canvas)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.overrideSorting = true;
    }

    public abstract void OnOpen(Transform parent);
    public abstract void OnClose(Transform parent, Action onCompleteClose = null);
}

public class StaticUI : BaseUI
{
    public override void OnOpen(Transform parent)
    {
        transform.SetParent(parent, false);
        gameObject.SetActive(true);
    }

    public override void OnClose(Transform parent, Action onCompleteClose = null)
    {
        gameObject.SetActive(false);
        transform.SetParent(parent, false);
        onCompleteClose?.Invoke();
    }
}

public class PopupUI : BaseUI
{
    public DOTweenAnimation openAnim;
    public DOTweenAnimation closeAnim;

    private Coroutine closeCoroutine;

    public override void OnOpen(Transform parent)
    {
        transform.SetParent(parent, false);
        gameObject.SetActive(true);
        
        if (openAnim)
        {
            openAnim.DOPlay();
        }
    }

    public override void OnClose(Transform parent, Action onCompleteClose = null)
    {
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        transform.SetParent(parent, false);

        if (!closeAnim)
        {
            gameObject.SetActive(false);
            onCompleteClose?.Invoke();
        }
        else
        {
            closeAnim.onComplete.AddListener(() => {
                closeAnim.onComplete.RemoveAllListeners();
                gameObject.SetActive(false);
                closeCoroutine = null;
                onCompleteClose?.Invoke();
            });

            closeAnim.DOPlay();
        }
    }

    public void CloseThis()
    {
        uiSystem.ClosePopup(this);
    }
}




