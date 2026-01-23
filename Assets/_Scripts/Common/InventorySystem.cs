using System;
using System.Collections.Generic;

public struct InventorySystemInitData
{
    public int SlotCapacity;
    public List<Item> InitialItems;
}

public class InventorySlot
{
    private Func<InventorySlot, IReadOnlyDictionary<SerialNumber, Item>> getItemDicFunc;

    public ItemData ItemData { get; private set; }

    public IEnumerable<Item> Items
    {
        get
        {
            return getItemDicFunc(this).Values;
        }
    }

    public int ItemCount
    {
        get
        {
            return getItemDicFunc(this).Count;
        }
    }

    public InventorySlot(ItemData itemData, Func<InventorySlot, IReadOnlyDictionary<SerialNumber, Item>> getItemDicFunc)
    {
        ItemData = itemData;
        this.getItemDicFunc = getItemDicFunc;
    }

    public Item GetFirstItem(Predicate<Item> predicate = null)
    {
        var itemDic = getItemDicFunc(this);
        foreach (var item in itemDic.Values)
        {
            if (predicate == null || predicate(item))
            {
                return item;
            }
        }

        return null;
    }
}

public class InventorySystem
{
    class TransactionScope : IDisposable
    {
        private InventorySystem inventorySystem;

        public TransactionScope(InventorySystem inventorySystem)
        {
            this.inventorySystem = inventorySystem;
            inventorySystem.transacting = true;
            inventorySystem.shouldNotifyChanges = false;
        }

        public void Dispose()
        {
            inventorySystem.transacting = false;
            if (inventorySystem.shouldNotifyChanges)
            {
                inventorySystem.OnInventoryChanged?.Invoke();
                inventorySystem.shouldNotifyChanges = false;
            }
        }
    }

    private int slotCapacity;
    private List<InventorySlot> slots = new();
    private Dictionary<InventorySlot, Dictionary<SerialNumber, Item>> slotItems = new();
    private Dictionary<SerialNumber, InventorySlot> serialToSlot = new();
    private Dictionary<ItemData, HashSet<InventorySlot>> itemDataToSlot = new();
    private bool transacting;
    private bool shouldNotifyChanges;

    public bool IsInfinite => slotCapacity == 0;
    public int SlotCount => slots.Count;
    public int MaxSlotCapacity => slotCapacity;
    public IReadOnlyList<InventorySlot> Slots => slots.AsReadOnly();

    public event Action OnInventoryChanged;

    public InventorySystem(InventorySystemInitData? initData = null)
    {
        if (initData.HasValue)
        {
            var data = initData.Value;
            slotCapacity = data.SlotCapacity;
            if (data.InitialItems != null)
            {
                AddItems(data.InitialItems);
            }
        }
        else
        {
            slotCapacity = 0;
        }
    }

    public IDisposable BeginTransaction()
    {
        return new TransactionScope(this);
    }

    private bool CanCreateSlot()
    {
        return IsInfinite || slots.Count < slotCapacity;
    }

    private InventorySlot TryCreateSlot(ItemData itemData)
    {
        if (CanCreateSlot())
        {
            var newSlot = new InventorySlot(itemData, (slot) => slotItems[slot]);
            slots.Add(newSlot);
            slotItems[newSlot] = new Dictionary<SerialNumber, Item>();

            if (!itemDataToSlot.TryGetValue(itemData, out var slotSet))
            {
                slotSet = new HashSet<InventorySlot>();
                itemDataToSlot[itemData] = slotSet;
            }
            slotSet.Add(newSlot);

            return newSlot;
        }

        return null;
    }

    private void RegisterItem(Item item, InventorySlot slot)
    {
        slotItems[slot][item.SerialNumber] = item;
        serialToSlot[item.SerialNumber] = slot;
    }

    private void UnregisterItem(Item item, InventorySlot slot)
    {
        var itemsInSlot = slotItems[slot];
        itemsInSlot.Remove(item.SerialNumber);

        serialToSlot.Remove(item.SerialNumber);
        if (itemsInSlot.Count == 0)
        {
            slots.Remove(slot);
            slotItems.Remove(slot);

            var slotSet = itemDataToSlot[slot.ItemData];
            slotSet.Remove(slot);
            if (slotSet.Count == 0)
            {
                itemDataToSlot.Remove(slot.ItemData);
            }
        }
    }

    public bool IsSlotFull(InventorySlot slot)
    {
        if (!slotItems.TryGetValue(slot, out var itemList))
        {
            throw new ArgumentException("Slot not found in inventory.");
        }

        if (!slot.ItemData.IsStackable)
        {
            return itemList.Count >= 1;
        }
        else if (slot.ItemData.IsInfiniteStackSize)
        {
            return false;
        }

        return itemList.Count >= slot.ItemData.MaxStackSize;
    }

    public bool CanAddItem(InventorySlot slot, Item item)
    {
        if (!slots.Contains(slot))
        {
            Logger.Warn("Slot not found in inventory.");
            return false;
        }

        if (slot.ItemData != item.ItemData)
        {
            return false;
        }

        return !IsSlotFull(slot);
    }

    public bool CanAddItem(Item item)
    {
        InventorySlot availableSlot = GetAvailableSlot(item.ItemData);
        if (availableSlot != null)
        {
            return true;
        }

        return CanCreateSlot();
    }

    public void CanAddItems(IEnumerable<Item> items, out List<Item> canAddItems, out List<Item> cannotAddItems)
    {
        canAddItems = new List<Item>();
        cannotAddItems = new List<Item>();

        foreach (var item in items)
        {
            if (CanAddItem(item))
            {
                canAddItems.Add(item);
            }
            else
            {
                cannotAddItems.Add(item);
            }
        }
    }

    public InventorySlot AddItem(Item item)
    {
        InventorySlot availableSlot = GetAvailableSlot(item.ItemData);
        if (availableSlot == null)
        {
            availableSlot = TryCreateSlot(item.ItemData);
            if (availableSlot == null)
            {
                Logger.Warn("No available slot to add item: " + item.ItemData.DisplayName);
                return null;
            }
        }

        RegisterItem(item, availableSlot);

        if (!transacting)
        {
            OnInventoryChanged?.Invoke();
        }
        else
        {
            shouldNotifyChanges = true;
        }

        return availableSlot;
    }

    public void AddItems(IEnumerable<Item> items)
    {
        bool changed = false;

        InventorySlot availableSlot = null;
        foreach (var item in items)
        {
            if (availableSlot != null)
            {
                if (!CanAddItem(availableSlot, item))
                {
                    availableSlot = null;
                }
            }
            else
            {
                availableSlot = GetAvailableSlot(item.ItemData);
            }

            if (availableSlot == null)
            {
                availableSlot = TryCreateSlot(item.ItemData);
                if (availableSlot == null)
                {
                    Logger.Warn("No available slot to add item: " + item.ItemData.DisplayName);
                    return;
                }
            }

            RegisterItem(item, availableSlot);

            changed = true;
        }

        if (changed)
        {
            if (transacting)
            {
                shouldNotifyChanges = true;
            }
            else
            {
                OnInventoryChanged?.Invoke();
            }
        }
    }

    private InventorySlot GetAvailableSlot(ItemData itemData)
    {
        foreach (var slot in slots)
        {
            if (slot.ItemData == itemData && !IsSlotFull(slot))
            {
                return slot;
            }
        }

        return null;
    }

    public bool RemoveItem(Item item)
    {
        InventorySlot targetSlot  = GetSlotByItem(item);
        return targetSlot != null ? RemoveItem(targetSlot, item) : false;
    }

    public bool RemoveItem(InventorySlot slot, Item item)
    {
        if (!slots.Contains(slot) || !serialToSlot.ContainsKey(item.SerialNumber))
        {
            Logger.Warn("Slot or item not found in inventory.");
            return false;
        }

        UnregisterItem(item, slot);

        if (!transacting)
        {
            OnInventoryChanged?.Invoke();
        }
        else
        {
            shouldNotifyChanges = true;
        }

        return true;
    }

    public void RemoveItems(IEnumerable<Item> items)
    {
        bool changed = false;
        foreach (var item in items)
        {
            InventorySlot targetSlot = GetSlotByItem(item);
            if (targetSlot == null)
            {
                continue;
            }

            UnregisterItem(item, targetSlot);
            changed = true;
        }

        if (changed)
        {
            if (transacting)
            {
                shouldNotifyChanges = true;
            }
            else
            {
                OnInventoryChanged?.Invoke();
            }
        }
    }

    public Item GetItem(SerialNumber serialNumber)
    {
        if (serialToSlot.TryGetValue(serialNumber, out var slot))
        {
            return slotItems[slot][serialNumber];
        }

        return null;
    }

    public List<Item> GetItems(ItemData itemData, Predicate<Item> filter = null)
    {
        return GetItems(itemData, int.MaxValue, filter);
    }

    public List<Item> GetItems(ItemData itemData, int maxCount, Predicate<Item> filter = null)
    {
        int remainingCount = maxCount;
        List<Item> items = new();
        if (itemDataToSlot.TryGetValue(itemData, out var slotSet))
        {
            foreach (var slot in slotSet)
            {
                foreach (var (_, item) in slotItems[slot])
                {
                    if (filter != null && !filter(item))
                    {
                        continue;
                    }

                    items.Add(item);
                    if (--remainingCount <= 0)
                    {
                        return items;
                    }
                }
            }
        }
        return items;
    }

    public Item GetFirstItem(ItemData itemData, Predicate<Item> filter = null)
    {
        var items = GetItems(itemData, 1, filter);
        return items.Count > 0 ? items[0] : null;
    }

    public InventorySlot GetSlotByItem(Item item)
    {
        return serialToSlot.TryGetValue(item.SerialNumber, out var slot) ? slot : null;
    }

    public bool MoveItemToOtherSlot(Item item, InventorySlot targetSlot)
    {
        InventorySlot currentSlot = GetSlotByItem(item);
        if (currentSlot == null || currentSlot == targetSlot)
        {
            Logger.Warn("Item not found in inventory or already in target slot.");
            return false;
        }

        if (targetSlot.ItemData != item.ItemData)
        {
            Logger.Warn("Target slot item data does not match item.");
            return false;
        }

        if (IsSlotFull(targetSlot))
        {
            Logger.Warn("Target slot is full.");
            return false;
        }

        UnregisterItem(item, currentSlot);
        RegisterItem(item, targetSlot);

        if (!transacting)
        {
            OnInventoryChanged?.Invoke();
        }
        else
        {
            shouldNotifyChanges = true;
        }

        return true;
    }

    public int GetItemCountInSlot(InventorySlot slot)
    {
        if (slotItems.TryGetValue(slot, out var itemList))
        {
            return itemList.Count;
        }

        return 0;
    }

    public int GetTotalItemCount(ItemData itemData, Predicate<Item> filter = null)
    {
        int totalCount = 0;
        if (itemDataToSlot.TryGetValue(itemData, out var slotSet))
        {
            if (filter == null)
            {
                foreach (var slot in slotSet)
                {
                    totalCount += slotItems[slot].Count;
                }
            }
            else
            {
                foreach (var slot in slotSet)
                {
                    foreach (var (_, item) in slotItems[slot])
                    {
                        if (filter(item))
                        {
                            totalCount++;
                        }
                    }
                }
            }
        }
        return totalCount;
    }

    public void Clear()
    {
        slots.Clear();
        slotItems.Clear();
        serialToSlot.Clear();
        itemDataToSlot.Clear();

        if (!transacting)
        {
            OnInventoryChanged?.Invoke();
        }
        else
        {
            shouldNotifyChanges = true;
        }
    }
}