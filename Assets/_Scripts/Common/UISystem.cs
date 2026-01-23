using System;
using System.Collections.Generic;
using UnityEngine;

public enum UISortingOrder
{
    DefaultStatic = 1,
    DefaultPopup,
    BelowOverlayPopup,
    OverlayPopup,
    BelowTopMost,
    TopMost,
}

public class UISystem : AppSystem
{
    static readonly int SortingOrderFactor = 100;

    public Transform closedTransform;
    public Transform openedTransform;

    private ResourcesSystem resourcesSystem;

    private Dictionary<Type, StaticUI> staticUIs = new();

    private List<StaticUI> activeStaticUIs = new();
    private LinkedList<PopupUI> activePopupUIs = new();

    public PopupUI TopPopup
    {
        get
        {
            if (activePopupUIs.Count > 0)
            {
                return activePopupUIs.Last.Value;
            }
            return null;
        }
    }

    public override void OnAttach(AppManager manager)
    {
        resourcesSystem = manager.GetSystem<ResourcesSystem>();
    }

    public override void OnDetach()
    {
        CloseAllPopups();
        CloseAllStatics(true);
    }

    private T CreateUIInstance<T>() where T : BaseUI
    {
        T instance = null;
        if (resourcesSystem.TryGetPrefab(typeof(T).Name, out var prefab))
        {
            instance = Instantiate(prefab, openedTransform).GetComponent<T>();
        }

        return instance;
    }

    private int GetSortingOrderOffset(UISortingOrder sortingOrder)
    {
        return ((int)sortingOrder) * SortingOrderFactor;
    }

    private int GetSortingOrderOffset(int sortingOrder)
    {
        return (sortingOrder / SortingOrderFactor) * SortingOrderFactor;
    }

    public T OpenStatic<T>() where T : StaticUI
    {
        var alreadyActive = activeStaticUIs.Find(ui => ui is T);
        if (alreadyActive != null)
        {
            return alreadyActive as T;
        }

        int sortingOrder = GetSortingOrderOffset(UISortingOrder.DefaultStatic) + staticUIs.Count;

        if (staticUIs.TryGetValue(typeof(T), out var existingUI))
        {
            activeStaticUIs.Add(existingUI);
            existingUI.SortingOrder = sortingOrder;
            existingUI.OnOpen(openedTransform);

            return existingUI as T;
        }

        var instance = CreateUIInstance<T>();
        if (!instance)
        {
            Logger.Error($"UIManager::OpenStaticUI {typeof(T)} prefab not found in Resources");
            return null;
        }

        staticUIs.Add(typeof(T), instance);
        activeStaticUIs.Add(instance);
        instance.Init(this);
        instance.SortingOrder = sortingOrder;
        instance.OnOpen(openedTransform);

        return instance;
    }

    public void CloseStatic<T>(bool destroy = true) where T : StaticUI
    {
        CloseStatic(typeof(T), destroy);
    }

    public void CloseStatic(Type type, bool destroy = true)
    {
        var active = activeStaticUIs.Find(ui => ui.GetType() == type);

        if (active != null)
        {
            int sortingOrderOffset = GetSortingOrderOffset(UISortingOrder.DefaultStatic);

            activeStaticUIs.Remove(active);
            for (int i = 0; i < activeStaticUIs.Count; i++)
            {
                var ui = activeStaticUIs[i];
                ui.SortingOrder = sortingOrderOffset + i;
            }

            active.OnClose(closedTransform);
            if (destroy)
            {
                staticUIs.Remove(type);
                Destroy(active.gameObject);
            }
        }
    }

    public void CloseAllStatics(bool destroy = true)
    {
        foreach (var ui in activeStaticUIs)
        {
            ui.OnClose(closedTransform);
        }
        activeStaticUIs.Clear();

        if (destroy)
        {
            foreach (var ui in staticUIs.Values)
            {
                Destroy(ui.gameObject);
            }
            staticUIs.Clear();
        }
    }

    public T GetActiveStatic<T>() where T : StaticUI
    {
        foreach (var ui in activeStaticUIs)
        {
            if (ui is T tUI)
            {
                return tUI;
            }
        }
        return null;
    }

    public T OpenPopup<T>(UISortingOrder sortingOrder = UISortingOrder.DefaultPopup) where T : PopupUI
    {
        var instance = CreateUIInstance<T>();
        if (!instance)
        {
            Logger.Error($"UIManager::OpenPopupUI {typeof(T)} prefab not found in Resources");
            return null;
        }

        int nextSortingOrderOffset = GetSortingOrderOffset(sortingOrder + 1);
        int expectSortingOrder = GetSortingOrderOffset(sortingOrder);
        LinkedListNode<PopupUI> insertBeforeNode = null;

        var node = activePopupUIs.First;
        while (node != null)
        {
            var popup = node.Value;

            if (popup.SortingOrder >= nextSortingOrderOffset)
            {
                insertBeforeNode = node;
                break;
            }

            if (expectSortingOrder == popup.SortingOrder)
            {
                ++expectSortingOrder;
            }

            node = node.Next;
        }

        if (insertBeforeNode != null)
        {
            activePopupUIs.AddBefore(insertBeforeNode, instance);
        }
        else
        {
            activePopupUIs.AddLast(instance);
        }

        instance.Init(this);
        instance.SortingOrder = expectSortingOrder;
        instance.OnOpen(openedTransform);

        return instance;
    }

    public List<T> GetActivePopups<T>() where T : PopupUI
    {
        List<T> result = new List<T>();
        foreach (var popup in activePopupUIs)
        {
            if (popup is T tPopup)
            {
                result.Add(tPopup);
            }
        }
        return result;
    }

    public T GetFirstActivePopup<T>() where T : PopupUI
    {
        foreach (var popup in activePopupUIs)
        {
            if (popup is T tPopup)
            {
                return tPopup;
            }
        }
        return null;
    }

    public void MakePopupToTop(PopupUI popup, bool ignoreLayerRestriction = false)
    {
        var node = activePopupUIs.First;
        while (node != null)
        {
            if (node.Value == popup)
            {
                int nextSortingOrderOffset;
                if (ignoreLayerRestriction)
                {
                    nextSortingOrderOffset = int.MaxValue;
                }
                else
                {
                    int currentSortingOrderOffset = GetSortingOrderOffset(popup.SortingOrder);
                    nextSortingOrderOffset = currentSortingOrderOffset + SortingOrderFactor;
                }

                var currentNode = node;
                var nextNode = currentNode.Next;
                while (nextNode != null && nextNode.Value.SortingOrder < nextSortingOrderOffset)
                {
                    int tempSortingOrder = currentNode.Value.SortingOrder;
                    currentNode.Value.SortingOrder = nextNode.Value.SortingOrder;
                    nextNode.Value.SortingOrder = tempSortingOrder;

                    PopupUI tempPopup = currentNode.Value;
                    currentNode.Value = nextNode.Value;
                    nextNode.Value = tempPopup;

                    currentNode = nextNode;
                    nextNode = currentNode.Next;    
                }

                break;
            }

            node = node.Next;
        }
    }

    public bool AnyPopupActive()
    {
        return activePopupUIs.Count > 0;
    }

    public void ClosePopup(PopupUI popup)
    {
        var node = activePopupUIs.First;

        while (node != null)
        {
            if (node.Value == popup)
            {
                int nextSortingOrderOffset = GetSortingOrderOffset(popup.SortingOrder);

                var tempNode = node.Next;
                while (tempNode != null && tempNode.Value.SortingOrder < nextSortingOrderOffset)
                {
                    --tempNode.Value.SortingOrder;
                    tempNode = tempNode.Next;
                }

                activePopupUIs.Remove(node);
                popup.OnClose(closedTransform, () => { Destroy(popup.gameObject); });

                break;
            }

            node = node.Next;
        }
    }

    public void CloseTopPopup()
    {
        if (activePopupUIs.Count == 0)
        {
            return;
        }
        var topPopup = activePopupUIs.Last.Value;
        activePopupUIs.RemoveLast();
        topPopup.OnClose(closedTransform, () => { Destroy(topPopup.gameObject); });
    }

    public void CloseAllPopups()
    {
        while (activePopupUIs.Count > 0)
        {
            CloseTopPopup();
        }
    }

    public override Type[] GetDependencySystemTypes()
    {
        return new Type[]
        {
            typeof(ResourcesSystem)
        };
    }
}