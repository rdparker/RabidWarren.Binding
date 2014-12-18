using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding
{
    [BindingConverter(typeof(int), typeof(string))]
    public class IntegerConverter : IBindingConverter
    {
        public object ConvertTo(object value, Type targetType, object parameter)
        {
            var n = (int)value;
            return n.ToString();
        }

        public object ConvertFrom(object value, Type targetType, object parameter)
        {
            var strValue = value as string;
            int result;

            if (int.TryParse(strValue, out result))
            {
                return result;
            }

            return null;
        }
    }
}
