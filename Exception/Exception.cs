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
        /// Instantiates an instance of this class with the given exception message.
        /// </summary>
        public Exception(string message) : base(message)
        {
        }
    }
}
