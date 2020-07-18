﻿using System;
using System.Collections.Generic;
using SE.Utility;

namespace SE.Pooling
{
    public class ObjectPool<T> where T : new()
    {
        public PoolBehavior Behaviour = PoolBehavior.Grow;

        public int Count => PoolCount + ActiveCount;
        public int PoolCount => Pool.Count;
        public int ActiveCount => Active.Count;

        protected QuickList<T> Pool;
        protected HashSet<T> Active;

        private Func<T> instantiateFunc;

        public virtual T Take()
        {
            T obj;
            IPoolable pooled = null;
            if (Pool.Count < 1) {
                switch (Behaviour) {
                    case PoolBehavior.Grow:
                        obj = instantiateFunc != null ? instantiateFunc.Invoke() : new T();
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

        public virtual void Return(T obj)
        {
            if (obj == null || !Active.Contains(obj))
                return;

            Active.Remove(obj);
            Pool.Add(obj);
            if (obj is IPoolable pooled) {
                pooled.ReturnedToPool();
            }
        }

        public ObjectPool(int startingCapacity = 128)
        {
            Pool = new QuickList<T>(startingCapacity);
            Active = new HashSet<T>();
            for (int i = 0; i < startingCapacity; i++) {
                T obj = new T();
                if (obj is IPoolable pooled) {
                    pooled.PoolInitialize();
                }
                Pool.Add(obj);
            }
        }

        public ObjectPool(Func<T> instantiateFunc, int startingCapacity = 128)
        {
            this.instantiateFunc = instantiateFunc;
            Pool = new QuickList<T>(startingCapacity);
            Active = new HashSet<T>();
            for (int i = 0; i < startingCapacity; i++) {
                T obj = instantiateFunc.Invoke();
                if (obj is IPoolable pooled) {
                    pooled.PoolInitialize();
                }
                Pool.Add(obj);
            }
        }
    }
}