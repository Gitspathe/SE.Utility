using SE.Core.Extensions;
using SE.Utility;
using SE.Attributes;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SE.Core
{
    public static class ReflectionUtil
    {
        private static bool isDirty = true;
        private static bool initialized;
        private static HashSet<Type> allTypes = new HashSet<Type>();
        private static HashSet<Assembly> loadedAssemblies = new HashSet<Assembly>();
        private static Dictionary<Type, HashSet<Type>> cachedTypes = new Dictionary<Type, HashSet<Type>>();

        public static void Initialize()
        {
            if (initialized)
                return;

            // Each time a new assembly is loaded into the domain, set the dirty flag to true.
            AppDomain.CurrentDomain.AssemblyLoad += (o, args) => {
                isDirty = true;
            };
            initialized = true;
        }

        public static QuickList<Type> GetTypes(Func<Type, bool> predicate)
        {
            if (isDirty)
                Setup();

            QuickList<Type> types = new QuickList<Type>();
            foreach (Type type in allTypes) {
                if (predicate(type)) {
                    types.Add(type);
                }
            }

            return types;
        }

        public static QuickList<T> GetTypeInstances<T>(Func<Type, bool> predicate)
        {
            if (isDirty)
                Setup();

            QuickList<Type> types = GetTypes(predicate);
            QuickList<T> qList = new QuickList<T>(allTypes.Count);
            foreach (Type t in types) {
                qList.Add((T)Activator.CreateInstance(t));
            }
            return qList;
        }

        public static QuickList<Type> GetCachedTypes(Type cached, Func<Type, bool> predicate)
        {
            if (isDirty)
                Setup();

            QuickList<Type> types = new QuickList<Type>();
            if (cachedTypes.TryGetValue(cached, out HashSet<Type> cachedList)) {
                foreach (Type type in cachedList) {
                    if (predicate(type)) {
                        types.Add(type);
                    }
                }
            }
            return types;
        }

        public static QuickList<T> GetCachedTypeInstances<T>(Func<Type, bool> predicate)
        {
            if (isDirty)
                Setup();

            QuickList<Type> types = GetCachedTypes(typeof(T), predicate);
            QuickList<T> qList = new QuickList<T>(allTypes.Count);
            foreach (Type t in types) {
                qList.Add((T)Activator.CreateInstance(t));
            }
            return qList;
        }

        /// <summary>
        /// Clears and resets all reflection cache data.
        /// </summary>
        internal static void FullReset()
        {
            // TODO: Call this when the editor reloads assemblies.
            cachedTypes.Clear();
            allTypes.Clear();
            loadedAssemblies.Clear();
            Setup();
        }

        private static void Setup()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                if (loadedAssemblies.Contains(assembly)) {
                    continue;
                }

                loadedAssemblies.Add(assembly);
                allTypes.AddRange(assembly.GetTypes());
            }

            Parallel.ForEach(allTypes, (type) => {
                object[] attributes = type.GetCustomAttributes(typeof(ReflectionCachedType), true);
                for (int i = 0; i < attributes.Length; i++) {
                    ReflectionCachedType cacheInfo = (ReflectionCachedType)attributes[i];
                    Type baseType = cacheInfo.Type;
                    Type realType = type;

                    if (!cacheInfo.IncludeBase && realType == baseType) {
                        continue;
                    }

                    lock (cachedTypes) {
                        if (cachedTypes.TryGetValue(baseType, out HashSet<Type> typeList)) {
                            typeList.Add(realType);
                        } else {
                            typeList = new HashSet<Type>() { realType };
                            cachedTypes.Add(baseType, typeList);
                        }
                    }

                }
            });

            isDirty = false;
        }

    }
}
