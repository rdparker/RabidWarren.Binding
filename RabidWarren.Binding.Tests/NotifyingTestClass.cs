namespace RabidWarren.Binding.Tests
{
    internal class NotifyingTestClass : NotifyingObject
    {
        private string _raisesByExpression;

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

        private string _raisesByString;
    }
}