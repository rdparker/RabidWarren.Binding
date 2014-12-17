// -----------------------------------------------------------------------
//  <copyright file="IObservableObject.cs" company="Ron Parker">
//   Copyright 2014 Ron Parker
//  </copyright>
//  <summary>
//   Defines the intereface for observable objects.  They must raise the
//   PropertyChanged event.
//  </summary>
// -----------------------------------------------------------------------

namespace Binding
{
    using System.ComponentModel;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    public interface IObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Raises the property changed event event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        void OnPropertyChangedEvent(string propertyName);
    }
}