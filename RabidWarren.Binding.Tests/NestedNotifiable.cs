// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NestedNotifiable.cs" company="Ron Parker">
//   Copyright 2015 Ron Parker
// </copyright>
// <summary>
//   Defines the NestedNotifiable type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RabidWarren.Binding.Tests
{
    using System.ComponentModel;

    class NestedNotifiable : INotifyPropertyChanged
    {
        string _text;

        public string Text
        {
            get
            {
                return _text;
            }

            set
            {
                _text = value;

                var notify = PropertyChanged;
                if (notify != null)
                    notify(this, new PropertyChangedEventArgs("Text"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
