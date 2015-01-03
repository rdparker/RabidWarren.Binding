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
        static Dictionary<Tuple<Type, Type>, Type> _registry;

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the binding converter registry. </summary>
        ///
        /// <value> The binding converter registry. </value>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public static Dictionary<Tuple<Type, Type>, Type> Registry
        {
            get
            {
                // The first time the registry is accessed, register all the default converters that are
                // supplied with it.  The existence of the registry signals that this registration has already
                // occured and provides a mechanism to trigger it. 
                if (_registry == null)
                {
                    _registry = new Dictionary<Tuple<Type, Type>, Type>();

                    RegisterAllConverters(typeof(ConverterRegistry).Assembly);
                }

                return _registry;
            }
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Registers all of the binding converters found within <paramref name="assembly"/>.
        /// <para>
        /// Binding converters are used to convert between properties of different types, for example an
        /// integer and a text field.  Their implementations must have the
        /// <see cref="BindingConverterAttribute"/> and they must implement the
        /// <see cref="IBindingConverter"/> interface.</para>
        /// <para>
        /// Any assembly which contains binding converters must call this method to register all of its
        /// converters.</para>
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 1/3/2015. </remarks>
        ///
        /// <param name="assembly"> The assembly containing the binding converters. </param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void RegisterAllConverters(Assembly assembly)
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
                _registry.Add(
                    Tuple.Create(entry.SourceType, entry.TargetType),
                    entry.converter);
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Searches for a binding converter from <paramref name="sourceType"/> to
        /// <paramref name="targetType"/>.
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 1/3/2015. </remarks>
        ///
        /// <param name="sourceType">   Type of the source. </param>
        /// <param name="targetType">   Type of the target. </param>
        ///
        /// <returns>   The type of the converter to be used. </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public static Type Find(Type sourceType, Type targetType)
        {
            Type converterType;

            if (Registry.TryGetValue(
                            Tuple.Create(sourceType, targetType),
                            out converterType))
            {
                return converterType;
            }
            else return null;
        }
    }
}
