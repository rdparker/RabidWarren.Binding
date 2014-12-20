// -----------------------------------------------------------------------
//  <copyright file="IBindingConverter.cs" company="Ron Parker">
//   Copyright 2014 Ron Parker
//  </copyright>
//  <summary>
//   Declares the interface for binding converter classes.
//  </summary>
// -----------------------------------------------------------------------

namespace Binding
{
    using System;

    /// <summary>
    /// Declares the interface binding converter classes must implement.  They must also have the
    /// <see cref="Binding.BindingConverterAttribute"/>.  If they do not
    /// <see cref="Binding.ConverterRegistry.RegisterAll()"/> will not find them.
    /// </summary>
    public interface IBindingConverter
    {
        /// <summary>
        /// Converts to the target type from the source type specified by the class
        /// <see cref="Binding.BindingConverterAttribute"/>.
        /// </summary>
        /// <returns>The converted value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target conversion type.</param>
        /// <param name="parameter">A user-supplied parameter.</param>
        object ConvertTo(object value, Type targetType, object parameter);

        /// <summary>
        /// Converts from the target type to the source type specified by the class
        /// <see cref="Binding.BindingConverterAttribute"/>.
        /// </summary>
        /// <returns>The converted value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target conversion type.</param>
        /// <param name="parameter">A user-supplied parameter.</param>
        object ConvertFrom(object value, Type targetType, object parameter);
    }
}
