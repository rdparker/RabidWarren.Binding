namespace Binding
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    
    public class BindingObject : ObservableObject
    {
        private ObservableObject _dataContext;
        private readonly Dictionary<string, string> _mappings = new Dictionary<string, string>();

        public BindingObject()
        {
        }

        public BindingObject(ObservableObject dataContext)
        {
            DataContext = dataContext;
        }

        public ObservableObject DataContext
        {
            get { return _dataContext; }

            set
            {
                if (_dataContext != null)
                {
                    _dataContext.PropertyChanged -= DataContextPropertyChanged;
                }

                _dataContext = value;
                _dataContext.PropertyChanged += DataContextPropertyChanged;
            }
        }

        private void DataContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateTargetFor(e.PropertyName);
        }

        private void UpdateTargetFor(string sourceName)
        {
            if (_mappings.ContainsKey(sourceName))
            {
                var target = _mappings[sourceName];
                var targetInfo = Properties[target];
                var sourceInfo = _dataContext.Properties[sourceName];

                if (targetInfo.Type == sourceInfo.Type)
                {
                    targetInfo.Set(targetInfo, sourceInfo.Get());
                }
                else
                {
                    var source = sourceInfo.Get();
                    targetInfo.Set(targetInfo, source.ToString());
                }
            }
        }

        public void Bind(string dataContextProperty, string targetProperty)
        {
            if (!_dataContext.Properties.ContainsKey(dataContextProperty))
            {
                var message = String.Format("\"{0}\" is not registered property of the data context.", dataContextProperty);

                throw new ArgumentException(message, "dataContextProperty");
            }

            if (!Properties.ContainsKey(targetProperty))
            {
                var message = String.Format("\"{0}\" is not a registered property of the binding object.", targetProperty);

                throw new ArgumentException(message, "targetProperty");
            }

            // Verify property compatability.
            var targetType = Properties[targetProperty].Type;
            if (targetType != typeof(string))
            {
                var sourceType = _dataContext.Properties[dataContextProperty].Type;
                if (targetType != sourceType)
                {
                    var message =
                        String.Format("Cannot bind a {0} property to a {1} property", sourceType, targetType);

                    throw new ArgumentException(message);
                }
            }

            _mappings.Add(dataContextProperty, targetProperty);

            // Set the initial value of the target property.
            UpdateTargetFor(dataContextProperty);
        }
    }
}
