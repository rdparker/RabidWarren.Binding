using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding
{
    using RabidWarren.Collections.Generic;
    using System.Reflection;

    public class ConverterRegistry
    {
        // Map the source type and target type to the converter type.
        internal static Dictionary<Tuple<Type, Type>, Type> Registry = new Dictionary<Tuple<Type,Type>,Type>();

        public static void RegisterAll(Assembly assembly)
        {


            //&&
            //        x.IsSubclassOf(typeof(IBindingConverter)));

            //foreach (var converter in converters)
            //{
            //    foreach (var attribute in converter.GetCustomAttributes(typeof(BindingConverterAttribute), false))
            //    {
            //        var binding = (BindingConverterAttribute)attribute;
            //        Registry.Add(
            //            Tuple.Create(binding.SourceType, binding.TargetType),
            //            converter);
            //    }
            //}

            var entries = from converter in assembly.GetTypes()
                          where converter.IsPublic && !converter.IsInterface && !converter.IsAbstract
                          let interfaces = converter.GetInterfaces()
                          from iface in interfaces
                          where iface == typeof(IBindingConverter)
                          let attributes = converter.GetCustomAttributes(typeof(BindingConverterAttribute), false)
                          from BindingConverterAttribute binding in attributes
                          select new { binding.SourceType, binding.TargetType, converter };

            foreach (var entry in entries)
                Registry.Add(Tuple.Create(entry.SourceType, entry.TargetType),
                    entry.converter);
        }
    }
}
