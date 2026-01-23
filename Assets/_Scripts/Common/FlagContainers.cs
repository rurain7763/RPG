using System;
using UnityEngine;

public static class FlagContainerCore
{
    public static bool Has(uint flags, uint flag)
    {
        return (flags & flag) != 0;
    }

    public static bool Any(uint flags, uint mask)
    {
        return (flags & mask) != 0;
    }

    public static bool All(uint flags, uint mask)
    {
        return (flags & mask) == mask;
    }

    public static void Add(ref uint flags, uint mask)
    {
        flags |= mask;
    }

    public static void Remove(ref uint flags, uint mask)
    {
        flags &= ~mask;
    }

    public static int Count(uint flags)
    {
#if false
        int count = 0;
        uint tempFlags = flags;
        while (tempFlags != 0)
        {
            count += (int)(tempFlags & 1);
            tempFlags >>= 1;
        }
        return count;
#else
        flags = flags - ((flags >> 1) & 0x55555555);
        flags = (flags & 0x33333333) + ((flags >> 2) & 0x33333333);
        return (int)((((flags + (flags >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24);
#endif
    }
}

[Serializable]
public struct UIntFlagContainer32
{
    [SerializeField] private uint flags;

    public bool Has(uint flag) => FlagContainerCore.Has(flags, flag);
    public bool Any(uint mask) => FlagContainerCore.Any(flags, mask);
    public bool All(uint mask) => FlagContainerCore.All(flags, mask);
    public void Add(uint mask) => FlagContainerCore.Add(ref flags, mask);
    public void Remove(uint mask) => FlagContainerCore.Remove(ref flags, mask);
    public int Count() => FlagContainerCore.Count(flags);
}

[Serializable]
public struct EnumFlagContainer32<T> where T : unmanaged, Enum
{
    [SerializeField] private uint flags;

    public bool Any(uint mask) => FlagContainerCore.Any(flags, mask);
    public bool All(uint mask) => FlagContainerCore.All(flags, mask);
    public void Add(uint mask) => FlagContainerCore.Add(ref flags, mask);
    public void Remove(uint mask) => FlagContainerCore.Remove(ref flags, mask);
    public int Count() => FlagContainerCore.Count(flags);

    public unsafe bool Has(T flag)
    {
        uint flagValue = *(uint*)&flag;
        return FlagContainerCore.Has(flags, flagValue);
    }

    public unsafe void Add(T flag)
    {
        uint flagValue = *(uint*)&flag;
        FlagContainerCore.Add(ref flags, flagValue);
    }

    public unsafe void Remove(T flag)
    {
        uint flagValue = *(uint*)&flag;
        FlagContainerCore.Remove(ref flags, flagValue);
    }
}