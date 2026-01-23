using System;
using System.Collections.Generic;

public interface IStat
{
    StatData StatData { get; }
    string DisplayName { get; }
    bool IsPercent { get; }
    float FinalValue { get; }

    event Action OnStatChanged;
}

public class Stat : IStat
{
    private StatData statData;
    private ModifiableValue modifiableValue;

    public StatData StatData => statData;

    public string DisplayName => statData.DisplayName;

    public bool IsPercent => statData.IsPercent;

    public float BaseValue
    {
        get => modifiableValue.BaseValue;
        set => modifiableValue.BaseValue = value;
    }

    public float FinalValue => Math.Clamp(modifiableValue.FinalValue, statData.MinValue, statData.MaxValue);

    public event Action OnStatChanged
    {
        add { modifiableValue.OnValueChanged += value; }
        remove { modifiableValue.OnValueChanged -= value; }
    }

    public Stat(StatData statData)
    {
        this.statData = statData;
        modifiableValue = new ModifiableValue();
    }

    public Stat(StatData statData, float baseValue)
    {
        this.statData = statData;
        modifiableValue = new ModifiableValue(baseValue);
    }

    public void AddModifier(ValueModifier modifier)
    {
        modifiableValue.AddModifier(modifier);
    }

    public void RemoveModifier(ValueModifier modifier)
    {
        modifiableValue.RemoveModifier(modifier);
    }
}

public class CombinedStat : IStat
{
    private StatData statData;

    private float finalValue;
    private bool isDirty;

    private List<Stat> stats = new();

    private Func<IReadOnlyList<Stat>, float> calculationFunc;

    public StatData StatData => statData;
    public string DisplayName => statData.DisplayName;
    public bool IsPercent => statData.IsPercent;

    public float FinalValue
    {
        get
        {
            if (isDirty)
            {
                CalculateFinalValue();
                isDirty = false;
            }

            return Math.Clamp(finalValue, statData.MinValue, statData.MaxValue);
        }
    }

    public event Action OnStatChanged;

    public CombinedStat(StatData statData)
    {
        this.statData = statData;
    }

    public CombinedStat(StatData statData, params Stat[] stats)
    {
        this.statData = statData;

        foreach (var stat in stats)
        {
            AddStat(stat);
        }
    }

    private void HandleStatChanged()
    {
        isDirty = true;
        OnStatChanged?.Invoke();
    }

    public void AddStat(Stat stat)
    {
        stat.OnStatChanged += HandleStatChanged;

        stats.Add(stat);
        HandleStatChanged();
    }

    public void RemoveStat(Stat stat)
    {
        if (stats.Remove(stat) == false)
        {
            return;
        }

        stat.OnStatChanged -= HandleStatChanged;
        HandleStatChanged();
    }

    public void SetCalculationFunc(Func<IReadOnlyList<Stat>, float> calculationFunc)
    {
        this.calculationFunc = calculationFunc;
        HandleStatChanged();
    }

    public void CalculateFinalValue()
    {
        finalValue = 0f;

        if (calculationFunc != null)
        {
            finalValue = calculationFunc(stats);
        }
        else
        {
            foreach (var stat in stats)
            {
                finalValue += stat.FinalValue;
            }
        }
    }
}