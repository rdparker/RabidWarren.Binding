using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding
{
    public class PropertyInfo
    {
        public PropertyInfo()
        {
        }

        public PropertyInfo(string name, Func<object> getter, Action<PropertyInfo, object> setter)
        {
            Name = name;
            Get = getter;
            Set = setter;

            CanGet = HasGetter;
            CanSet = HasSetter;
        }

        public string Name { get; protected set; }
        public Type Type { get; protected set; }
        public Func<object> Get { get; protected set; }
        public Action<PropertyInfo, object> Set { get; protected set; }

        public Func<bool> CanGet { get; set; }
        public Func<bool> CanSet { get; set; }

        public bool HasGetter() { return Get != null; }
        public bool HasSetter() { return Set != null; }
    }

    public class PropertyInfo<T> : PropertyInfo
    {
        public PropertyInfo(string name, Func<T> getter)
        {
            base.Name = name;
            base.Get = () => { return (object)getter(); };

            Type = typeof(T);
            Get = getter;
        }

        public PropertyInfo(string name, Action<PropertyInfo<T>, T> setter)
        {
            base.Name = name;
            base.Set = (property, value) => setter((PropertyInfo<T>)property, (T)value);

            Type = typeof(T);
            Set = setter;
        }

        public PropertyInfo(string name, Func<T> getter, Action<PropertyInfo<T>, T> setter)
        {
            base.Name = name;
            base.Get = () => { return (object)getter(); };
            base.Set = (property, value) => setter((PropertyInfo<T>)property, (T)value);

            Type = typeof(T);
            Get = getter;
            Set = setter;
        }

        public new Func<T> Get { get; protected set; }
        public new Action<PropertyInfo<T>, T> Set { get; protected set; }
    }
}
