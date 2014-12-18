using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding
{
    interface IBindingConverter
    {
        object ConvertTo(object value, Type targetType, object parameter);
        object ConvertFrom(object value, Type targetType, object parameter);
    }
}
