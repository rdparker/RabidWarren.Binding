// -----------------------------------------------------------------------
//  <copyright file="Exception.cs" company="Ron Parker">
//   Copyright 2015 Ron Parker
//  </copyright>
//  <summary>
//   Provides Exception classes for RabidWarren.Binding.
//  </summary>
// -----------------------------------------------------------------------

namespace RabidWarren.Binding
{
    /// <summary>
    /// Serves as a base class for all RabidWarren.Binding exceptions.
    /// </summary>
    class Exception : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Exception"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        protected Exception(string message) : base(message)
        {
        }
    }
}
