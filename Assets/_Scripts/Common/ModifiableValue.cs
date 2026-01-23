using System;
using System.Collections.Generic;
using UnityEngine;

public enum DefaultValueModiferOrder
{
    Override = 100,
    Clamp = 200,
    Mult = 300,
    CategoryTotalMult = 400,
    TotalMult = 500,
    MaxMult = 600,
    MinMult = 700,
    Add = 800,
}

public abstract class ValueModifier
{
    public float Value;
    public int Priority;

    private ValueModifier next;

    internal ValueModifier Next
    {
        get => next;
        set => next = value;
    }

    public ValueModifier() { }

    public ValueModifier(float value, int priority)
    {
        Value = value;
        Priority = priority;
    }

    abstract public ValueModifier Calculate(ref float currentValue);
}

public class AddValueModifier : ValueModifier
{
    public AddValueModifier()
        : base(0, (int)DefaultValueModiferOrder.Add)
    {
    }

    public AddValueModifier(float value, int priority = (int)DefaultValueModiferOrder.Add)
        : base(value, priority)
    {
    }

    public override ValueModifier Calculate(ref float currentValue)
    {
        currentValue += Value;

        return Next;
    }
}

public class MinMultValueModifier : ValueModifier
{
    public MinMultValueModifier()
        : base(0, (int)DefaultValueModiferOrder.MinMult)
    {
    }

    public MinMultValueModifier(float value, int priority = (int)DefaultValueModiferOrder.MinMult)
        : base(value, priority)
    {
    }

    public override ValueModifier Calculate(ref float currentValue)
    {
        float minValue = Value;

        ValueModifier iter = Next;
        while (iter is MinMultValueModifier si)
        {
            minValue = Mathf.Min(minValue, si.Value);
            iter = si.Next;
        }
        currentValue *= 1 + minValue;

        return iter;
    }
}

public class MaxMultValueModifier : ValueModifier
{
    public MaxMultValueModifier()
        : base(0, (int)DefaultValueModiferOrder.MaxMult)
    {
    }

    public MaxMultValueModifier(float value, int priority = (int)DefaultValueModiferOrder.MaxMult)
        : base(value, priority)
    {
    }

    public override ValueModifier Calculate(ref float currentValue)
    {
        float maxValue = Value;

        ValueModifier iter = Next;
        while (iter is MaxMultValueModifier si)
        {
            maxValue = Mathf.Max(maxValue, si.Value);
            iter = si.Next;
        }
        currentValue *= 1 + maxValue;

        return iter;
    }
}

public class TotalMultValueModifier : ValueModifier
{
    public TotalMultValueModifier()
        : base(0, (int)DefaultValueModiferOrder.TotalMult)
    {
    }

    public TotalMultValueModifier(float value, int priority = (int)DefaultValueModiferOrder.TotalMult)
        : base(value, priority)
    {
    }

    public override ValueModifier Calculate(ref float currentValue)
    {
        float sumValue = Value;

        ValueModifier iter = Next;
        while (iter is TotalMultValueModifier si)
        {
            sumValue += si.Value;
            iter = si.Next;
        }

        currentValue *= 1 + sumValue;

        return iter;
    }
}

public static class CategoryTotalMultValueModifierOffsetHolder
{
    public static int nextOffset = 0;
}

public class CategoryTotalMultValueModifier<T> : ValueModifier
{
    private static int offsetOrder = -1;

    public CategoryTotalMultValueModifier(float value)
        : base(value, (int)DefaultValueModiferOrder.CategoryTotalMult + GetOffset())
    {
    }

    public override ValueModifier Calculate(ref float currentValue)
    {
        float sumValue = Value;
        ValueModifier iter = Next;
        while (iter is CategoryTotalMultValueModifier<T> si)
        {
            sumValue += si.Value;
            iter = si.Next;
        }
        currentValue *= 1 + sumValue;
        return iter;
    }

    private static int GetOffset()
    {
        if (offsetOrder == -1)
        {
            offsetOrder = CategoryTotalMultValueModifierOffsetHolder.nextOffset++;
        }

        return offsetOrder;
    }
}

public class MultValueModifier : ValueModifier
{
    public MultValueModifier()
        : base(0, (int)DefaultValueModiferOrder.Mult)
    {
    }

    public MultValueModifier(float value, int priority = (int)DefaultValueModiferOrder.Mult)
        : base(value, priority)
    {
    }

    public override ValueModifier Calculate(ref float currentValue)
    {
        currentValue *= 1 + Value;
        return Next;
    }
}

public class OverrideValueModifier : ValueModifier
{
    public OverrideValueModifier()
        : base(0, (int)DefaultValueModiferOrder.Override)
    {
    }

    public OverrideValueModifier(float value, int priority = (int)DefaultValueModiferOrder.Clamp)
        : base(value, priority)
    {
    }

    public override ValueModifier Calculate(ref float currentValue)
    {
        currentValue = Value;
        return Next;
    }
}

public class ModifiableValue
{
    private float baseValue;
    private float finalValue;

    private ValueModifier firstModifier;
    private SortedDictionary<int, ValueModifier> lastModifiers = new(new Int32Greater());

    public float BaseValue
    {
        get => baseValue;
        set
        {
            baseValue = value;
            IsDirty = true;
            OnValueChanged?.Invoke();
        }
    }

    public float FinalValue
    {
        get
        {
            if (IsDirty)
            {
                CalculateFinalValue();
                IsDirty = false;
            }

            return finalValue;
        }
    }

    public bool IsDirty { get; private set; }

    public event Action OnValueChanged;

    public ModifiableValue()
    {
        BaseValue = 0;
    }

    public ModifiableValue(float baseValue)
    {
        BaseValue = baseValue;
    }

    public void AddModifier(ValueModifier modifier)
    {
        if (!lastModifiers.TryGetValue(modifier.Priority, out var lastMod))
        {
            if (firstModifier == null)
            {
                firstModifier = modifier;
            }
            else
            {
                bool inserted = false;
                ValueModifier prev = null;
                foreach (var (priority, last) in lastModifiers)
                {
                    if (priority < modifier.Priority)
                    {
                        if (prev == null)
                        {
                            modifier.Next = firstModifier;
                            firstModifier = modifier;
                        }
                        else
                        {
                            modifier.Next = prev.Next;
                            prev.Next = modifier;
                        }
                        inserted = true;
                        break;
                    }

                    prev = last;
                }

                if (!inserted)
                {
                    prev.Next = modifier;
                }
            }

            lastModifiers[modifier.Priority] = modifier;
        }
        else
        {
            lastMod.Next = modifier;
            lastModifiers[modifier.Priority] = modifier;
        }

        IsDirty = true;
        OnValueChanged?.Invoke();
    }

    public bool RemoveModifier(ValueModifier modifier)
    {
        if (firstModifier == null)
        {
            return false;
        }

        ValueModifier current = firstModifier;
        ValueModifier previous = null;
        while (current != null)
        {
            if (current == modifier)
            {
                if (previous != null)
                {
                    previous.Next = current.Next;

                    if (lastModifiers[current.Priority] == current)
                    {
                        if (previous.Priority == current.Priority)
                        {
                            lastModifiers[current.Priority] = previous;
                        }
                        else
                        {
                            lastModifiers.Remove(current.Priority);
                        }
                    }
                }
                else
                {
                    firstModifier = current.Next;

                    if (lastModifiers[current.Priority] == current)
                    {
                        lastModifiers.Remove(current.Priority);
                    }
                }

                IsDirty = true;
                break;
            }

            previous = current;
            current = current.Next;
        }

        if (IsDirty)
        {
            OnValueChanged?.Invoke();
        }

        return IsDirty;
    }

    public void RemoveAllModifiers(Predicate<ValueModifier> comparer)
    {
        if (firstModifier == null)
        {
            return;
        }

        ValueModifier current = firstModifier;
        ValueModifier previous = null;

        while (current != null)
        {
            if (comparer(current))
            {
                if (previous != null)
                {
                    previous.Next = current.Next;

                    if (lastModifiers[current.Priority] == current)
                    {
                        if (previous.Priority == current.Priority)
                        {
                            lastModifiers[current.Priority] = previous;
                        }
                        else
                        {
                            lastModifiers.Remove(current.Priority);
                        }
                    }
                }
                else
                {
                    firstModifier = current.Next;

                    if (lastModifiers[current.Priority] == current)
                    {
                        lastModifiers.Remove(current.Priority);
                    }
                }

                IsDirty = true;
            }
            else
            {
                previous = current;
            }

            current = current.Next;
        }

        if (IsDirty)
        {
            OnValueChanged?.Invoke();
        }
    }

    public void ClearModifiers()
    {
        firstModifier = null;
        lastModifiers.Clear();
        IsDirty = true;
        OnValueChanged?.Invoke();
    }

    public void CalculateFinalValue()
    {
        finalValue = BaseValue;

        ValueModifier current = firstModifier;
        while (current != null)
        {
            current = current.Calculate(ref finalValue);
        }
    }
}


