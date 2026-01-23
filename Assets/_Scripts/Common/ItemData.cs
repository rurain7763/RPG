using System;
using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public UUID ID;
    public string DisplayName;
    [TextArea] public string Description;
    public Sprite Icon;

    public abstract int Category { get; }

    public int MaxStackSize; // 0 means infinite stack size

    public bool IsStackable => MaxStackSize != 1;
    public bool IsInfiniteStackSize => MaxStackSize == 0;

    private void Awake()
    {
        if (ID.IsValid())
        {
            return;
        }    

        ID.Generate();
    }

    public abstract Item CreateItem();
    public abstract Item CreateItem(SerialNumber serialNumber);
}
