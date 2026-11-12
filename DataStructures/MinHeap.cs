using System;
using System.Collections.Generic;

namespace MunicipalServicesApplication.DataStructures
{
    // Array-backed min heap. Sequence number makes equal priorities FIFO.
    public sealed class MinHeap<T>
    {
        private sealed class Entry { internal T Value; internal int Priority; internal long Sequence; }
        private readonly List<Entry> items = new List<Entry>();
        private long sequence;
        public int Count { get { return items.Count; } }
        private static int Compare(Entry a, Entry b)
        {
            int order = a.Priority.CompareTo(b.Priority);
            return order != 0 ? order : a.Sequence.CompareTo(b.Sequence);
        }
        private void Swap(int a, int b) { var value = items[a]; items[a] = items[b]; items[b] = value; }
        public void Enqueue(T value, int priority)
        {
            items.Add(new Entry { Value = value, Priority = priority, Sequence = sequence++ });
            int index = items.Count - 1;
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (Compare(items[parent], items[index]) <= 0) break;
                Swap(index, parent); index = parent;
            }
        }
        public T Peek()
        {
            if (Count == 0) throw new InvalidOperationException("The heap is empty.");
            return items[0].Value;
        }
        public T Dequeue()
        {
            T value = Peek();
            items[0] = items[Count - 1];
            items.RemoveAt(Count - 1);
            int index = 0;
            while (index * 2 + 1 < Count)
            {
                int child = index * 2 + 1;
                if (child + 1 < Count && Compare(items[child + 1], items[child]) < 0) child++;
                if (Compare(items[index], items[child]) <= 0) break;
                Swap(index, child); index = child;
            }
            return value;
        }
    }
}
