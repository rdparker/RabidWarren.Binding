// -----------------------------------------------------------------------
//  <copyright file="PropertyMetadata.cs" company="Ron Parker">
//   Copyright 2014 Ron Parker
//  </copyright>
//  <summary>
//    Provides access to basic property metadata and functions for
//    accessing and manipulating a property's value.
//  </summary>
// -----------------------------------------------------------------------

namespace Binding
{
    using System;

    /// <summary>
    /// Provides access to basic property metadata and functions for accessing and manipulating a property's value.
    /// </summary>
    public class PropertyMetadata
    {
        /// <summary>
        /// Gets or sets the type of object containing the property.
        /// </summary>
        /// <value>The containing object's <see cref="System.Type"/>.</value>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the property's name.
        /// </summary>
        /// <value>The property's name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the function for getting the property's value.
        /// </summary>
        /// <value>The property's value getter.</value>
        public Func<object, object> Get { get; set; }

        /// <summary>
        /// Gets or sets the action performed to set the property's value.
        /// </summary>
        /// <value>The property's value setter.</value>
        public Action<object, object> Set { get;  set; }
    }
}
