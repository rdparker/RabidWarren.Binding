using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding
{
    public class PropertyDictionary : Dictionary<string, PropertyInfo>
    {
        private readonly Dictionary<string, PropertyInfo> _properties;

        public PropertyDictionary()
        {
            _properties = new Dictionary<string, PropertyInfo>();
        }
    }
}
