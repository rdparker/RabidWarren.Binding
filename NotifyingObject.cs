// -----------------------------------------------------------------------
//  <copyright file="NotifyingObject.cs" company="Ron Parker">
//   Copyright 2014 Ron Parker
//  </copyright>
//  <summary>
//   Provides the base implementation for raising the PropertyChanged
//   event.
//  </summary>
// -----------------------------------------------------------------------

namespace Binding
{
    using System.ComponentModel;

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
    }
}
