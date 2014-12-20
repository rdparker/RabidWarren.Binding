// -----------------------------------------------------------------------
//  <copyright file="BindingConverter.cs" company="Ron Parker">
//   Copyright 2014 Ron Parker
//  </copyright>
//  <summary>
//   Provides a base implementation for binding converters.
//  </summary>
// -----------------------------------------------------------------------

namespace Binding
{
    using System;

    /// <summary>
    /// Declares the methods binding converter classes must implement.  They must also have the
    /// <see cref="Binding.BindingConverterAttribute"/>.  If they do not
    /// <see cref="Binding.ConverterRegistry.RegisterAll()"/> will not find them.
    /// </summary>
    public abstract class BindingConverter : IBindingConverter
    {
        /// <summary>
        /// Specifies a value that is used by the binding system instead of null to indicate that a property has not
        /// been set by the property system.  This is done because null may be a valid property value.
        /// </summary>
        public static readonly object NoValue = new object();

        /// <summary>
        /// Converts to the target type from the source type specified by the class
        /// <see cref="Binding.BindingConverterAttribute"/>.
        /// </summary>
        /// <returns>The converted value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target conversion type.</param>
        /// <param name="parameter">A user-supplied parameter.</param>
        public abstract object ConvertTo(object value, Type targetType, object parameter);

        /// <summary>
        /// Converts from the target type to the source type specified by the class
        /// <see cref="Binding.BindingConverterAttribute"/>.
        /// </summary>
        /// <returns>The converted value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target conversion type.</param>
        /// <param name="parameter">A user-supplied parameter.</param>
        public abstract object ConvertFrom(object value, Type targetType, object parameter);
    }
}