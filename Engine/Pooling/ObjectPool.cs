using SE.Utility;
using System;
using System.Collections.Generic;

namespace SE.Pooling
{
    public abstract class ObjectPoolBase<T>
    {
        public PoolBehavior Behaviour = PoolBehavior.Grow;

        public int Count => PoolCount + ActiveCount;
        public int PoolCount => Pool.Count;
        public int ActiveCount => Active.Count;

        protected QuickList<T> Pool;
        protected HashSet<T> Active;

        protected ObjectPoolBase(int startingCapacity = 128)
        {
            Pool = new QuickList<T>(startingCapacity);
            Active = new HashSet<T>();
        }

        public T Take()
        {
            T obj;
            IPoolable pooled = null;
            if (Pool.Count < 1) {
                switch (Behaviour) {
                    case PoolBehavior.Grow:
                        obj = CreateNewInstance();
                        pooled = obj as IPoolable;
                        pooled?.PoolInitialize();
                        break;
                    case PoolBehavior.Fixed:
                        return default;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            } else {
                obj = Pool.Take(Pool.Count - 1);
            }

            Active.Add(obj);
            pooled?.TakenFromPool();
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null || !Active.Contains(obj))
                return;

            Active.Remove(obj);
            Pool.Add(obj);
            if (obj is IPoolable pooled) {
                pooled.ReturnedToPool();
            }
        }

        protected abstract T CreateNewInstance();
    }

    public class ObjectPool<T> : ObjectPoolBase<T> where T : new()
    {
        public ObjectPool(int startingCapacity = 128) : base(startingCapacity)
        {
            for (int i = 0; i < startingCapacity; i++) {
                T obj = new T();
                if (obj is IPoolable pooled) {
                    pooled.PoolInitialize();
                }
                Pool.Add(obj);
            }
        }

        protected override T CreateNewInstance()
        {
            return new T();
        }
    }

    public class FuncObjectPool<T> : ObjectPoolBase<T>
    {
        private Func<T> instantiateFunc;

        public FuncObjectPool(Func<T> instantiateFunc, int startingCapacity = 128) : base(startingCapacity)
        {
            this.instantiateFunc = instantiateFunc;
            for (int i = 0; i < startingCapacity; i++) {
                T obj = instantiateFunc.Invoke();
                if (obj is IPoolable pooled) {
                    pooled.PoolInitialize();
                }
                Pool.Add(obj);
            }
        }

        protected override T CreateNewInstance()
        {
            return instantiateFunc.Invoke();
        }
    }
}
