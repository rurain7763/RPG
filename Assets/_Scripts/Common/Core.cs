using System;
using System.Collections.Generic;
using UnityEngine;

public interface ICondition
{
    bool Evaluate();
}

public class FuncCondition : ICondition
{
    Func<bool> func;

    public FuncCondition(Func<bool> func)
    {
        this.func = func;
    }

    public bool Evaluate()
    {
        return func();
    }
}

public class Int32Greater : IComparer<int>
{
    public int Compare(int x, int y)
    {
        return y.CompareTo(x);
    }
}

public class Int32Less : IComparer<int>
{
    public int Compare(int x, int y)
    {
        return x.CompareTo(y);
    }
}

public interface IWeightedItem
{
    int Weight { get; }
}

[Serializable]
public struct SerialNumber : IEquatable<SerialNumber>
{
    [SerializeField] private ulong value;

    public SerialNumber(ulong value)
    {
        this.value = value;
    }

    public bool Equals(SerialNumber other)
    {
        return value == other.value;
    }

    public override bool Equals(object obj)
    {
        return obj is SerialNumber other && Equals(other);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }

    public static bool operator ==(SerialNumber left, SerialNumber right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SerialNumber left, SerialNumber right)
    {
        return !left.Equals(right);
    }

    public static implicit operator ulong(SerialNumber serialNumber)
    {
        return serialNumber.value;
    }

    public static implicit operator SerialNumber(ulong value)
    {
        return new SerialNumber(value);
    }

    public bool IsValid()
    {
        return value != 0;
    }

    public static SerialNumber CreateSimple()
    {
        return ulong.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 99999).ToString("D4"));
    }
}

public abstract class Arguments
{
}

public interface IEventData
{
    object Value { get; }
}

[Serializable]
public class StringEventData : IEventData
{
    [SerializeField] private string value;

    public string Value => value;
    object IEventData.Value => value;
}

[Serializable]
public class IntEventData : IEventData
{
    [SerializeField] private int value;

    public int Value => value;
    object IEventData.Value => value;
}

[Serializable]
public class FloatEventData : IEventData
{
    [SerializeField] private float value;

    public float Value => value;
    object IEventData.Value => value;
}

[Serializable]
public class TransformEventData : IEventData
{
    [SerializeField] private Transform value;
    public Transform Value => value;
    object IEventData.Value => value;
}

[Serializable]
public class Line2D
{
    [SerializeField] private Vector2[] points;

    public Vector2[] Points => points;

    public Line2D()
    {
        points = Array.Empty<Vector2>();
    }

    public Line2D(params Vector2[] points)
    {
        this.points = points;
    }

    public void AddPoints(params Vector2[] newPoints)
    {
        int originalLength = points.Length;
        Array.Resize(ref points, originalLength + newPoints.Length);
        Array.Copy(newPoints, 0, points, originalLength, newPoints.Length);
    }

    public void Clear()
    {
        points = Array.Empty<Vector2>();
    }
}
