using System;
using System.Security.Cryptography;
using UnityEngine;

[Serializable]
public struct UUID
{
    [SerializeField] private ulong value;

    public ulong Value => value;

    public UUID(ulong value = 0)
    {
        this.value = value;
    }

    public void Generate()
    {
        byte[] buffer = new byte[8];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(buffer);
        }

        value = BitConverter.ToUInt64(buffer, 0);

        if (value == 0)
        {
            value++;
        }
    }

    public bool IsValid()
    {
        return value != 0;
    }

    public override bool Equals(object obj)
    {
        if (obj is UUID uuid)
        {
            return this == uuid;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }

    public override string ToString()
    {
        return value.ToString();
    }

    public static bool operator ==(UUID a, UUID b)
    {
        return a.value == b.value;
    }

    public static bool operator !=(UUID a, UUID b)
    {
        return a.value != b.value;
    }

    public static explicit operator ulong(UUID uuid)
    {
        return uuid.value;
    }

    public static explicit operator UUID(ulong value)
    {
        return new UUID(value);
    }

    public static UUID Create()
    {
        UUID uuid = new UUID();
        uuid.Generate();
        return uuid;
    }
}