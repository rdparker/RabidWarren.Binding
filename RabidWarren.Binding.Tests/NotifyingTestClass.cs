// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotifyingTestClass.cs" company="Ron Parker">
//   Copyright 2015 Ron Parker
// </copyright>
// <summary>
//   Defines the NotifyingTestClass type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RabidWarren.Binding.Tests
{
    class NotifyingTestClass : NotifyingObject
    {
        string _raisesByExpression;
        string _raisesByString;

        public string RaisesByExpression
        {
            get { return _raisesByExpression; }
            set
            {
                this.SetProperty(() => RaisesByExpression, value, v => _raisesByExpression = v);
                _raisesByExpression = value;
            }
        }

        public string RaisesByString
        {
            get { return _raisesByString; }
            set
            {
                _raisesByString = value;
                RaisePropertyChanged("RaisesByString");
            }
        }
    }
}