using System;
using System.Collections.Generic;
using UnityEngine;

public enum BuffCategory : byte
{
    Neutral,
    Positive,
    Negative,
}

public abstract class Buff
{
    public uint ID { get; protected set; } // if higher ID buffs should be applied first
    public BuffCategory Category { get; protected set; }
    public float Duration
    {
        get => duration;
        set
        {
            if (Mathf.Approximately(duration, value))
            {
                return;
            }

            duration = value;
            OnDurationChanged?.Invoke();
        }
    }

    public int StackCount
    {
        get => stackCount;
        set
        {
            if (stackCount == value)
            {
                return;
            }

            stackCount = value;
            OnStackCountChanged?.Invoke();
        }
    }

    public object Owner { get; set; }
    public object Source { get; set; }

    [SerializeField] private float duration;
    [SerializeField] private int stackCount;

    public event Action OnDurationChanged;
    public event Action OnStackCountChanged;

    public Buff(uint id, BuffCategory category, float duration = -1f, int stackCount = 1, object source = null)
    {
        ID = id;
        Category = category;
        Duration = duration;
        StackCount = stackCount;
        Source = source;
    }

    public bool IsInfinite()
    {
        return Duration < 0;
    }

    virtual public void OnApply() { }
    virtual public void OnTick() { }
    virtual public void OnExpire() { }

    virtual public bool OnDuplicate(Buff newBuff)
    {
        return false;
    }

    virtual public bool IsExpired()
    {
        return !IsInfinite() && Duration <= 0;
    }
}

public class BuffSystem
{
    private List<LinkedList<Buff>> buffLists = new();
 
    public object Owner { get; private set; }

    public BuffSystem(object owner)
    {
        Owner = owner;
    }

    public void AddBuff(Buff newBuff)
    {
        newBuff.Owner = Owner;

        int listIndex = (int)newBuff.ID;

        if (buffLists.Count <= listIndex)
        {
            while (buffLists.Count <= listIndex)
            {
                buffLists.Add(new LinkedList<Buff>());
            }
        }

        var buffList = buffLists[listIndex];
        for (LinkedListNode<Buff> node = buffList.First; node != null; node = node.Next)
        {
            Buff existing = node.Value;

            if (existing.IsExpired())
            {
                continue;
            }

            if (existing.ID == newBuff.ID && existing.OnDuplicate(newBuff))
            {
                return;
            }
        }

        newBuff.OnApply();
        buffList.AddLast(newBuff);
    }

    public void RemoveAllBuffs(Predicate<Buff> match)
    {
        foreach(var buffList in buffLists)
        {
            LinkedListNode<Buff> node = buffList.First;
            while (node != null)
            {
                Buff buff = node.Value;
                LinkedListNode<Buff> nextNode = node.Next;
                if (match(buff))
                {
                    buff.OnExpire();
                    buffList.Remove(node);
                }
                node = nextNode;
            }
        }
    }

    public void RemoveAllBuffsWithID(uint buffID)
    {
        RemoveAllBuffs(buff => buff.ID == buffID);
    }

    public void RemoveAllBuffWithCategory(BuffCategory category)
    {
        RemoveAllBuffs(buff => buff.Category == category);
    }

    public void Tick(float delta)
    {
        // Iterate backwards to apply higher id buffs first
        foreach (var buffList in buffLists)
        {
            LinkedListNode<Buff> node = buffList.Last;
            while (node != null)
            {
                Buff buff = node.Value;
                LinkedListNode<Buff> prevNode = node.Previous;
                buff.OnTick();
                if (!buff.IsInfinite())
                {
                    buff.Duration = Mathf.Max(0, buff.Duration - delta);
                }

                if (buff.IsExpired())
                {
                    buff.OnExpire();
                    buffList.Remove(node);
                }

                node = prevNode;
            }
        }
    }

    public bool HasBuff(uint buffID)
    {
        int listIndex = (int)buffID;
        if (buffLists.Count <= listIndex)
        {
            return false;
        }

        var buffList = buffLists[listIndex];
        foreach (var buff in buffList)
        {
            if (buff.ID == buffID && !buff.IsExpired())
            {
                return true;
            }
        }

        return false;
    }

    public List<Buff> GetAllBuffs(uint buffID)
    {
        List<Buff> result = new List<Buff>();
        
        int listIndex = (int)buffID;
        if (buffLists.Count <= listIndex)
        {
            return result;
        }
        
        var buffList = buffLists[listIndex];
        foreach (var buff in buffList)
        {
            if (buff.ID == buffID && !buff.IsExpired())
            {
                result.Add(buff);
            }
        }

        return result;
    }

    public Buff GetFirstBuff(uint buffID)
    {
        int listIndex = (int)buffID;
        if (buffLists.Count <= listIndex)
        {
            return null;
        }
        
        var buffList = buffLists[listIndex];
        foreach (var buff in buffList)
        {
            if (buff.ID == buffID && !buff.IsExpired())
            {
                return buff;
            }
        }

        return null;
    }

    public void EachBuffs(Action<Buff> action)
    {
        for (int i = buffLists.Count - 1; i >= 0; i--)
        {
            var buffList = buffLists[i];
            foreach (var buff in buffList)
            {
                if (buff.IsExpired())
                {
                    // Skip expired buffs
                    continue;
                }

                action(buff);
            }
        }
    }
}