using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public sealed class BindingConverterAttribute : Attribute
    {
        public BindingConverterAttribute(Type sourceType, Type targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;
        }

        public Type SourceType { get; private set; }

        public Type TargetType { get; private set; }

        public Type ParameterType { get; private set; }
    }
}
