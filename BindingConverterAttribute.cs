// -----------------------------------------------------------------------
//  <copyright file="BindingConverterAttribute.cs" company="Ron Parker">
//   Copyright 2014 Ron Parker
//  </copyright>
//  <summary>
//   Provides a BindingConverterAttribute for marking converter classes.
//  </summary>
// -----------------------------------------------------------------------

namespace Binding
{
    using System;

    /// <summary>
    /// Represents a binding converter attribute, which is used to mark classes that perform conversion between types
    /// for the portable binding mechanism.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class BindingConverterAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Binding.BindingConverterAttribute"/> class.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <param name="targetType">The target type.</param>
        public BindingConverterAttribute(Type sourceType, Type targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;
        }

        /// <summary>
        /// Gets the type of the source property.
        /// </summary>
        /// <value>The type of the source property.</value>
        public Type SourceType { get; private set; }

        /// <summary>
        /// Gets the type of the target property.
        /// </summary>
        /// <value>The type of the target property.</value>
        public Type TargetType { get; private set; }

        /// <summary>
        /// Gets the type of the optional parameter.
        /// </summary>
        /// <value>The type of the parameter.</value>
        public Type ParameterType { get; private set; }
    }
}