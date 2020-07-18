using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming

namespace SE.Utility
{
    /// <summary>
    /// IMPORTANT: YOU -MUST- CALL DISPOSE() WHEN FINISHED WITH POOLED LISTS. FAILING TO DO SO WILL CAUSE MEMORY LEAKS!
    /// 
    /// Optimized list which exposed direct access to it's underlying array.
    /// Additionally, this type can optionally use ArrayPool in order to reduce GC allocations.
    /// Exercise caution when working with the inner array, as there are no sanity checks.
    /// PooledList is not thread-safe. Remember to wrap it in locks when writing multi-threaded code.
    /// </summary>
    /// <typeparam name="T">Type of the inner array.</typeparam>
    public sealed class PooledList<T> : IEnumerable<T>, IDisposable
    {
        /// <summary>Inner array/buffer.</summary>
        public T[] Array;

        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        public bool UseArrayPool => useArrayPool;

        /// <summary>Elements length. DO NOT MODIFY!</summary>
        public int Count;

        private int bufferLength; // Faster to cache the array length.
        private bool useArrayPool;
        private bool disposeContents;
        private bool isDisposed;

        /// <summary>
        /// Creates a new PooledList instance.
        /// </summary>
        /// <param name="useArrayPool">Whether or not to use ArrayPool. Don't be stupid with it or you'll get memory leaks!!</param>
        /// <param name="disposeContents">Whether or not to dispose of contents when Dispose() is called.</param>
        /// <param name="size">Initial capacity. Automatically resizes.</param>
        public PooledList(bool useArrayPool, bool disposeContents, int size)
        {
            this.useArrayPool = useArrayPool;
            this.disposeContents = disposeContents;
            size = Math.Max(1, size);
            Array = useArrayPool
                ? ArrayPool<T>.Shared.Rent(size)
                : new T[size];

            bufferLength = Array.Length;
        }

        public PooledList(bool useArrayPool = true, bool disposeContents = true) : this(useArrayPool, disposeContents, 8) { }

        /// <summary>
        /// NOTE: Directly accessing the Buffer is preferred, as that would be faster.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [Obsolete("It's preferred to access the inner array directly via the " + nameof(Array) + " field.")]
        public T this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Array[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Array[index] = value;
        }

        /// <summary>
        /// Resets the length back to zero without clearing the items.
        /// </summary>
        public void Reset() => Count = 0;

        /// <summary>
        /// Clears the list of all it's items.
        /// </summary>
        public void Clear()
        {
            System.Array.Clear(Array, 0, Array.Length);
            Count = 0;
        }

        /// <summary>
        /// Adds an item.
        /// </summary>
        /// <param name="item">The item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // Hot path.
        public void Add(T item)
        {
            if (Count < bufferLength) {
                Array[Count++] = item;
            } else {
                AddWithResize(item);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // Cold path.
        private void AddWithResize(T item)
        {
            T[] newItems = useArrayPool 
                ? ArrayPool<T>.Shared.Rent(bufferLength * 2) 
                : new T[bufferLength * 2];

            System.Array.Copy(Array, 0, newItems, 0, bufferLength);
            if (useArrayPool) {
                ArrayPool<T>.Shared.Return(Array);
            }

            Array = newItems;
            bufferLength = Array.Length;
            Array[Count++] = item;
        }

        public void AddRange(PooledList<T> items)
        {
            EnsureCapacity(items.Count);
            System.Array.Copy(items.Array, 0, Array, Count, items.Count);
            Count += items.Count;
        }

        public void AddRange(List<T> items)
        {
            EnsureCapacity(items.Count);
            items.CopyTo(0, Array, Count, items.Count);
            Count += items.Count;
        }

        public void AddRange(T[] items)
        {
            EnsureCapacity(items.Length);
            System.Array.Copy(items, 0, Array, Count, items.Length);
            Count += items.Length;
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // Cold path.
        public void EnsureCapacity(int additionalItems = 1)
        {
            if (Count + additionalItems >= bufferLength) {
                T[] newItems = useArrayPool 
                    ? ArrayPool<T>.Shared.Rent(bufferLength + additionalItems) 
                    : new T[bufferLength + additionalItems];

                System.Array.Copy(Array, 0, newItems, 0, bufferLength);
                if (useArrayPool) {
                    ArrayPool<T>.Shared.Return(Array);
                }

                Array = newItems;
                bufferLength = Array.Length;
            }
        }

        /// <summary>
        /// Adds a enumerable range of items.
        /// </summary>
        /// <param name="enumerable">IEnumerable collection.</param>
        public void AddRange(IEnumerable<T> enumerable)
        {
            foreach (T item in enumerable)
                Add(item);
        }

        /// <summary>
        /// Sorts the list with a given comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public void Sort(IComparer comparer) 
            => System.Array.Sort(Array, 0, Count, comparer);

        /// <summary>
        /// Sorts the list with a given comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public void Sort(IComparer<T> comparer)
            => System.Array.Sort(Array, 0, Count, comparer);

        /// <summary>
        /// Checks if an item is present in the list. Uses the default equality comparer to test.
        /// </summary>
        /// <param name="item">Item to check.</param>
        /// <returns>True if the item is present, false otherwise.</returns>
        public bool Contains(T item)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < Count; ++i) {
                if (comparer.Equals(item, Array[i])) {
                    return true;
                }
            }
            return false;
        }

        public void Insert(int index, T item)
        {
            if (Count == Array.Length) {
                EnsureCapacity(1);
            }
            if (index < Count) {
                System.Array.Copy(Array, index, Array, index + 1, Count - index);
            }
            Array[index] = item;
            Count++;
        }

        /// <summary>
        /// Removes an element at a specific index.
        /// </summary>
        /// <param name="index">Index of item to remove.</param>
        public void RemoveAt(int index)
        {
            Count--;
            if(index < Count)
                System.Array.Copy(Array, index + 1, Array, index, Count - index);

            Array[Count] = default;
        }

        /// <summary>
        /// Removes an item from the list.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        /// <returns>True if the item was present and was removed, false otherwise.</returns>
        public bool Remove(T item)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < Count; ++i) {
                if (comparer.Equals(item, Array[i])) {
                    RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds the index of an item. Returns -1 if the item isn't found.
        /// </summary>
        /// <param name="item">Item to find the index of.</param>
        /// <returns>Index of the element, if found. If not found, returns -1.</returns>
        public int IndexOf(T item)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < Count; ++i) {
                if (comparer.Equals(item, Array[i])) {
                    RemoveAt(i);
                    return i;
                }
            }
            return -1;
        }

        public void Dispose()
        {
            //GC.SuppressFinalize(this);
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if(isDisposed)
                return;

            if (disposeContents && typeof(IDisposable).IsAssignableFrom(typeof(T))) {
                foreach (IDisposable disposable in this) {
                    disposable.Dispose();
                }
            }

            if (useArrayPool) {
                ArrayPool<T>.Shared.Return(Array);
            }
            isDisposed = true;
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private struct Enumerator : IEnumerator<T>
        {
            private PooledList<T> PooledList;
            private int curIndex;

            public bool MoveNext()
            {
                curIndex++;
                return (curIndex < PooledList.Count);
            }

            public void Reset() => curIndex = -1;

            public T Current => PooledList.Array[curIndex];

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public Enumerator(PooledList<T> PooledList)
            {
                curIndex = -1;
                this.PooledList = PooledList;
            }
        }

        // Finalizer worsens GCs. :(
        //~PooledList()
        //{
        //    Dispose(false);
        //}
    }
}
