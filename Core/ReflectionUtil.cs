using SE.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SE.Core
{
    public static class ReflectionUtil
    {
        private static bool isDirty = true;
        private static bool initialized;
        private static HashSet<Type> allTypes = new HashSet<Type>();

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

        public static IEnumerable<Type> GetTypes(Func<Type, bool> predicate)
        {
            if (isDirty)
                Setup();

            return allTypes.Where(predicate);
        }

        public static IEnumerable<T> GetTypeInstances<T>(Func<Type, bool> predicate)
        {
            if (isDirty)
                Setup();

            IEnumerable<Type> types = GetTypes(predicate);
            List<T> enumerable = new List<T>(allTypes.Count);
            foreach (Type t in types) {
                enumerable.Add((T)Activator.CreateInstance(t));
            }
            return enumerable;
        }

        private static void Setup()
        {
            allTypes.Clear();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                allTypes.AddRange(assembly.GetTypes());
            }
            isDirty = false;
        }
    }
}
