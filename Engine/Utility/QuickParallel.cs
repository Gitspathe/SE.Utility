using SE.Pooling;
using System;
using System.Buffers;
using System.Collections;
using System.Threading;

namespace SE.Utility
{
    public static class QuickParallel
    {
        private static ThreadLocal<CountdownEvent> countdownLocal
            = new ThreadLocal<CountdownEvent>(() => new CountdownEvent(1));

        // TODO: Still not as fast as Parallel :(
        public static void ForEach<T>(QuickList<T> source, Action<T> action)
        {
            int threads = Math.Max(Environment.ProcessorCount - 1, 1);
            int amount = (int) Math.Floor((double) source.Count / threads);
            int curOffset = 0;

            CountdownEvent countdown = countdownLocal.Value;
            countdown.Reset(threads);
            for (int i = 0; i < threads; i++) {
                if (threads == 1 || i == threads - 1) {
                    amount = source.Count - curOffset;
                }
                QueueThread(source.Array, curOffset, amount, countdown, action);
                curOffset += amount;
            }
            countdown.Wait();
        }

        public static void ForEach<T>(QuickList<T> source, Action<T[], int> action)
        {
            int threads = Math.Max(Environment.ProcessorCount - 1, 1);
            int amount = (int) Math.Floor((double) source.Count / threads);
            int curOffset = 0;

            CountdownEvent countdown = countdownLocal.Value;
            countdown.Reset(threads);
            for (int i = 0; i < threads; i++) {
                if (threads == 1 || i == threads - 1) {
                    amount = source.Count - curOffset;
                }
                QueueThread(source.Array, curOffset, amount, countdown, action);
                curOffset += amount;
            }
            countdown.Wait();
        }

        public static void For(int fromInclusive, int toInclusive, Action<int> body)
        {
            int count = toInclusive - fromInclusive;
            int threads = Math.Max(Environment.ProcessorCount - 1, 1);
            int amount = (int) Math.Floor((double) count / threads);
            int curOffset = fromInclusive;

            CountdownEvent countdown = countdownLocal.Value;
            countdown.Reset(threads);
            for (int i = 0; i < threads; i++) {
                if (threads == 1 || i == threads - 1) {
                    amount = count - curOffset;
                }

                int offsetLocal = curOffset;
                int amountLocal = amount;
                ThreadPool.UnsafeQueueUserWorkItem(state => {
                    for (int ii = offsetLocal; ii < offsetLocal + amountLocal; ii++) {
                        body.Invoke(ii);
                    }
                    ((CountdownEvent) state).Signal();
                }, countdown);

                curOffset += amount;
            }
            countdown.Wait();
        }

        private static void QueueThread<T>(T[] source, int from, int count, CountdownEvent countdown, Action<T> action)
        {
            ThreadPool.UnsafeQueueUserWorkItem(state => {
                T[] array = ArrayPool<T>.Shared.Rent(count);
                Array.Copy(source, from, array, 0, count);

                for (int i = 0; i < count; i++) {
                    action.Invoke(array[i]);
                }

                ArrayPool<T>.Shared.Return(array);
                ((CountdownEvent) state).Signal();
            }, countdown);
        }

        private static void QueueThread<T>(T[] source, int from, int count, CountdownEvent countdown, Action<T[], int> action)
        {
            ThreadPool.UnsafeQueueUserWorkItem(state => {
                T[] array = ArrayPool<T>.Shared.Rent(count);
                Array.Copy(source, from, array, 0, count);

                action.Invoke(array, count);

                ArrayPool<T>.Shared.Return(array);
                ((CountdownEvent) state).Signal();
            }, countdown);
        }

        private class PooledCountdownEvent
        {
            public CountdownEvent Event;

            public PooledCountdownEvent()
            {
                Event = new CountdownEvent(1);
            }
        }
    }
}
