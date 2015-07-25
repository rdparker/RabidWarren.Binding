// -----------------------------------------------------------------------
//  <copyright file="IBindingConverter.cs" company="Ron Parker">
//   Copyright 2014, 2015 Ron Parker
//  </copyright>
//  <summary>
//   Declares the interface for binding converter classes.
//  </summary>
// -----------------------------------------------------------------------

namespace RabidWarren.Binding
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Declares the interface binding converter classes must implement.  They must also have the
    ///     <see cref="BindingConverterAttribute" />.  If they do not
    ///     <see cref="M:ConverterRegistry.RegisterAll()" /> will not find them.
    /// </summary>
    /// <remarks>
    ///     Instead of directly inheriting this interface, classes should be derived from
    ///     <see cref="Binding.BindingConverter" />.  It provides <see cref="Binding.BindingConverter.NoValue" />,
    ///     which is used to indicate that a property could not be converted between the two types.  This can happen
    ///     for example, when the user clears a numeric field that is bound to an integer before the enter the desired
    ///     value.
    /// </remarks>
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global",
        Justification = "ReSharper misses the implementation of this interface possibly due to the use of an " +
                        "abstract class in the hierarchy.")]
    [SuppressMessage("ReSharper", "UnusedParameter.Global", 
        Justification = "ReSharper misses the implementation of this interface possibly due to the use of an " +
                        "abstract class in the hierarchy.")]
    public interface IBindingConverter
    {
        /// <summary>
        ///     Converts to the target type from the source type specified by the class
        ///     <see cref="Binding.BindingConverterAttribute" />.
        /// </summary>
        /// <returns>The converted value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target conversion type.</param>
        /// <param name="parameter">A user-supplied parameter.</param>
        object ConvertTo(object value, Type targetType, object parameter);

        /// <summary>
        ///     Converts from the target type to the source type specified by the class
        ///     <see cref="Binding.BindingConverterAttribute" />.
        /// </summary>
        /// <returns>The converted value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target conversion type.</param>
        /// <param name="parameter">A user-supplied parameter.</param>
        object ConvertFrom(object value, Type targetType, object parameter);
    }
}
