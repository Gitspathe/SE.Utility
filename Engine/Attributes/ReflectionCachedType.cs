using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public class ReflectionCachedType : Attribute
    {
        public Type Type { get; set; }
        public bool IncludeBase { get; set; }

        public ReflectionCachedType(Type type, bool includeBase = true) : base()
        {
            Type = type;
            IncludeBase = includeBase;
        }
    }
}
