using System;
using System.Buffers;
using System.Collections;
using System.Threading;

namespace SE.Utility
{
    public static class QuickParallel
    {
        public static void ForEach<T>(QuickList<T> source, Action<T> action)
        {
            int threads = Environment.ProcessorCount;
            int amount = (int) Math.Floor((double) source.Count / threads);
            int curOffset = 0;
            for (int i = 0; i < threads; i++) {
                if (i == threads - 1) {
                    amount = source.Count - curOffset;
                }

                T[] array = ArrayPool<T>.Shared.Rent(amount);
                Array.Copy(source.Array, curOffset, array, 0, amount);

                int delegateAmount = amount;
                ThreadPool.QueueUserWorkItem(state => {
                    for (int ii = 0; ii < delegateAmount; ii++) {
                        action.Invoke(array[ii]);
                    }
                    ArrayPool<T>.Shared.Return(array);
                });
                curOffset += amount;
            }
        }
    }
}
