using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FastList<T> : IEnumerable<T>
{
    public struct Enumerator : IEnumerator<T>
    {
        private readonly FastList<T> _list;
        private int _index;
        private T _current;

        internal Enumerator(FastList<T> list)
        {
            _list = list;
            _index = 0;
            _current = default;
        }

        public bool MoveNext()
        {
            if (_index < _list.Count)
            {
                _current = _list[_index];
                _index++;
                return true;
            }
            return false;
        }

        public T Current => _current;
        object IEnumerator.Current => Current;

        public void Reset() { _index = 0; _current = default; }
        public void Dispose() { }
    }

    [SerializeField] private T[] items = Array.Empty<T>();
    [SerializeField] private int count = 0;

    public T this[int index] => items[index];
    public int Count => count;

    public void Add(T item)
    {
        if (count >= items.Length)
        {
            Array.Resize(ref items, Math.Max(4, items.Length * 2));
        }

        items[count++] = item;
    }

    public void Remove(T item)
    {
        int index = Array.IndexOf(items, item, 0, count);
        if (index == -1)
        {
            return;
        }

        RemoveAt(index);
    }

    public void RemoveAt(int index)
    {
        count--;
        items[index] = items[count];
        items[count] = default;
    }

    public bool Contains(T item)
    {
        return Array.IndexOf(items, item, 0, count) >= 0;
    }

    public int IndexOf(T item)
    {
        return Array.IndexOf(items, item, 0, count);
    }

    public bool Any(Predicate<T> predicate)
    {
        for (int i = 0; i < count; i++)
        {
            if (predicate(items[i]))
            {
                return true;
            }
        }

        return false;
    }

    public T FirstOrDefault(Predicate<T> predicate)
    {
        for (int i = 0; i < count; i++)
        {
            if (predicate(items[i]))
            {
                return items[i];
            }
        }

        return default;
    }

    public void Clear()
    {
        count = 0;
    }

    public Enumerator GetEnumerator() => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
}