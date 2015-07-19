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
                _raisesByExpression = value;
                RaisePropertyChanged(() => RaisesByExpression);
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