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
            int threads = Math.Min(Environment.ProcessorCount - 1, 1);
            int amount = (int) Math.Floor((double) source.Count / threads);
            int curOffset = 0;
            for (int i = 0; i < threads; i++) {
                if (i == threads - 1) {
                    amount = source.Count - curOffset;
                }

                T[] array = ArrayPool<T>.Shared.Rent(amount);
                Array.Copy(source.Array, curOffset, array, 0, amount);
                QueueThread(array, amount, action);
                
                curOffset += amount;
            }
        }

        public static void For(int fromInclusive, int toInclusive, Action<int> body)
        {
            int count = toInclusive - fromInclusive;
            int threads = Math.Min(Environment.ProcessorCount - 1, 1);
            int amount = (int) Math.Floor((double) count / threads);
            int curOffset = fromInclusive;
            for (int i = 0; i < threads; i++) {
                if (i == threads - 1) {
                    amount = count - curOffset;
                }

                for (int ii = curOffset; ii < curOffset + amount; ii++) {
                    body.Invoke(ii);
                }

                curOffset += amount;
            }
        }

        private static void QueueThread<T>(T[] array, int count, Action<T> action)
        {
            ThreadPool.QueueUserWorkItem(state => {
                for (int i = 0; i < count; i++) {
                    action.Invoke(array[i]);
                }
                ArrayPool<T>.Shared.Return(array);
            });
        }
    }
}
