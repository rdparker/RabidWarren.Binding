// -----------------------------------------------------------------------
//  <copyright file="IntegerConverter.cs" company="Ron Parker">
//   Copyright 2014 Ron Parker
//  </copyright>
//  <summary>
//   Converts back and forth between an integer and a string.
//  </summary>
// -----------------------------------------------------------------------

namespace Binding
{
    using System;

    /// <summary>
    /// Converts between <see cref="int"/> and <see cref="string"/>.
    /// </summary>
    [BindingConverter(typeof(int), typeof(string))]
    public class IntegerConverter : BindingConverter
    {
        /// <summary>
        /// Converts to a <see cref="string"/> from an <see cref="int"/>.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target conversion type.</param>
        /// <param name="parameter">A user-supplied parameter, which is ignored here.</param>
        public override object ConvertTo(object value, Type targetType, object parameter)
        {
            var n = (int)value;
            return n.ToString();
        }

        /// <summary>
        /// Converts from a <see cref="string"/> back to an <see cref="int"/>.
        /// </summary>
        /// <returns>The integer.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target conversion type (string).</param>
        /// <param name="parameter">A user-supplied parameter, which is ignored here.</param>        
        public override object ConvertFrom(object value, Type targetType, object parameter)
        {
            var strValue = value as string;
            int result;

            if (int.TryParse(strValue, out result))
            {
                return result;
            }

            return NoValue;
        }
    }
}
