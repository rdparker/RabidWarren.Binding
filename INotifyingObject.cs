// -----------------------------------------------------------------------
//  <copyright file="INotifyingObject.cs" company="Ron Parker">
//   Copyright 2014, 2015 Ron Parker
//  </copyright>
//  <summary>
//   Defines an extended interface for objects that implement
//   INotifyPropertyChange.
//  </summary>
// -----------------------------------------------------------------------

namespace RabidWarren.Binding
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    public interface INotifyingObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Raises the property changed event event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        [Obsolete("RabidWarren.Binding now uses RaisePropertyChanged.")]
        void OnPropertyChangedEvent(string propertyName);

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Raises the PropertyChanged event for the property named in the property expression.
        /// </summary>
        /// <typeparam name="TProperty">        The type of the object's property.</typeparam>
        /// <param name="propertyExpression">   The property expression.  These are generally of the form
        ///                                     <code>() => Property</code>.</param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        void RaisePropertyChanged<TProperty>(Expression<Func<TProperty>> propertyExpression);


        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Raises the PropertyChanged event for the named property.
        /// </summary>
        /// <param name="propertyName">         The property which changed.</param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        void RaisePropertyChanged(string propertyName);
    }
}