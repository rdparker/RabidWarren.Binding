// -----------------------------------------------------------------------
//  <copyright file="ConverterRegistry.cs" company="Ron Parker">
//   Copyright 2014 Ron Parker
//  </copyright>
//  <summary>
//   Provides a registry of binding converter classes.
//  </summary>
// -----------------------------------------------------------------------

namespace Binding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Houses the binding converter registry.
    /// </summary>
    public static class ConverterRegistry
    {
        /// <summary>
        /// Maps the source type and target type to the converter type.
        /// </summary>
        internal static Dictionary<Tuple<Type, Type>, Type> Registry = new Dictionary<Tuple<Type, Type>, Type>();

        /// <summary>
        /// Registers all of the binding converters found within <paramref name="assembly"/>.
        /// <para>Binding converters are classes which have the <see cref="Binding.BindingConverterAttribute"/>.  They
        /// must implement the <see cref="Binding.IBindingConverter"/> interface.</para>
        /// </summary>
        /// <param name="assembly">The assembly containing the binding converters.</param>
        public static void RegisterAll(Assembly assembly)
        {
            var entries = from converter in assembly.GetTypes()
                          where converter.IsPublic && !converter.IsInterface && !converter.IsAbstract
                          let interfaces = converter.GetInterfaces()
                          from iface in interfaces
                          where iface == typeof(IBindingConverter)
                          let attributes = converter.GetCustomAttributes(typeof(BindingConverterAttribute), false)
                          from BindingConverterAttribute binding in attributes
                          select new { binding.SourceType, binding.TargetType, converter };

            foreach (var entry in entries)
                Registry.Add(
                    Tuple.Create(entry.SourceType, entry.TargetType),
                    entry.converter);
        }
    }
}
