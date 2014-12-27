// -----------------------------------------------------------------------
//  <copyright file="INotifyingObject.cs" company="Ron Parker">
//   Copyright 2014 Ron Parker
//  </copyright>
//  <summary>
//   Defines an extended interface for objects that implement
//   INotifyPropertyChange.
//  </summary>
// -----------------------------------------------------------------------

namespace Binding
{
    using System.ComponentModel;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    public interface INotifyingObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Raises the property changed event event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        void OnPropertyChangedEvent(string propertyName);
    }
}