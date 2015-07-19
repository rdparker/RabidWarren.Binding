// -----------------------------------------------------------------------
//  <copyright file="NotifyingObject.cs" company="Ron Parker">
//   Copyright 2014, 2015 Ron Parker
//  </copyright>
//  <summary>
//   Provides the base implementation for raising the PropertyChanged
//   event.
//  </summary>
// -----------------------------------------------------------------------

namespace RabidWarren.Binding
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides the base implementation for raising the PropertyChanged event on objects which have properties that
    /// act as binding sources.
    /// </summary>
    public abstract class NotifyingObject : INotifyingObject
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed event.
        /// </summary>
        /// <param name="propertyName">The name of the property that was changed.</param>
        [Obsolete("RabidWarren.Binding now uses RaisePropertyChanged.")]
        public void OnPropertyChangedEvent(string propertyName)
        {
            // Avoid multithreaded race conditions where PropertyChanged could be updated between the test and
            // the call.
            var propertyChangedEvent = PropertyChanged;
            if (propertyChangedEvent != null)
            {
                propertyChangedEvent(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Raises the PropertyChanged event for the property named in the property expression.
        /// </summary>
        /// <typeparam name="TProperty">        The type of the object's property.</typeparam>
        /// <param name="propertyExpression">   The property expression.  These are generally of the form 
        ///                                     <code>() => Property</code>.</param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public void RaisePropertyChanged<TProperty>(Expression<Func<TProperty>> propertyExpression)
        {
            RaisePropertyChanged(propertyExpression.GetMemberName());
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Raises the PropertyChanged event for the named property.
        /// </summary>
        /// <param name="propertyName">         The property which changed.</param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
