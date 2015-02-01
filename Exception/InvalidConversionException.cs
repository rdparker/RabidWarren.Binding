// -----------------------------------------------------------------------
//  <copyright file="InvalidConversionException.cs" company="Ron Parker">
//   Copyright 2015 Ron Parker
//  </copyright>
//  <summary>
//   Provides an exception to be used when no appropriate BindingConverter
//   exists.
//  </summary>
// -----------------------------------------------------------------------

namespace RabidWarren.Binding
{
    /// <summary>
    /// An exception that is thrown when attempting to convert between property types with no known
    /// <see cref="T:BindingConverter"/>.
    /// </summary>
    class InvalidConversionException : Exception
    {
        /// <summary>
        /// Instantiates an instance of this class with the given exception message.
        /// </summary>
        /// <param name="message"></param>
        public InvalidConversionException(string message) : base(message)
        {
        }
    }
}
