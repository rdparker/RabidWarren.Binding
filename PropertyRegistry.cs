// -----------------------------------------------------------------------
//  <copyright file="PropertyRegistry.cs" company="Ron Parker">
//   Copyright 2014 Ron Parker
//  </copyright>
//  <summary>
//   Implements the central registry for bindable properties.
//  </summary>
// -----------------------------------------------------------------------
namespace Binding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using RabidWarren.Collections.Generic;

    /// <summary>
    ///  Implements the central registry for properties that may act as either the source or target of a binding.
    /// </summary>
    public static class PropertyRegistry
    {
        /// <summary>
        /// Maps class names to lists of properties.
        /// </summary>
        private static readonly Multimap<Type, PropertyMetadata> Registry = new Multimap<Type, PropertyMetadata>();

        /// <summary>
        /// Adds the named property and its getter to the property registry.
        /// <para>Also creates the IsGettable and IsSettable sub-properties for checking whether the property may be
        /// read and whether it may be written.</para>
        /// </summary>
        /// <typeparam name="TObject">The type of the object containing the property.</typeparam>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <param name="getter">The the function for getting the property's value from the object that
        /// contains it.</param>
        public static void Add<TObject, TValue>(string name, Func<TObject, TValue> getter)
            where TObject : class
        {
            Add(name, getter, null);
        }

        /// <summary>
        /// Adds the named property and its setter to the property registry.
        /// <para>Also creates the IsGettable and IsSettable sub-properties for checking whether the property may be
        /// read and whether it may be written.</para>
        /// </summary>
        /// <typeparam name="TObject">The type of the object containing the property.</typeparam>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <param name="setter">The the function for setting the property's value in the given object</param>
        /// <returns>The property's metadata.</returns>
        public static PropertyMetadata Add<TObject, TValue>(string name, Action<TObject, TValue> setter)
            where TObject : class
        {
            return Add(name, null, setter);
        }

        /// <summary>
        /// Adds the named property, its getter, and its setter to the property registry.
        /// <para>Also creates the IsGettable and IsSettable sub-properties for checking whether the property may be
        /// read and whether it may be written.</para>
        /// </summary>
        /// <typeparam name="TObject">The type of the object containing the property.</typeparam>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <param name="getter">The the function for getting the property's value from the object that
        /// contains it.</param>
        /// <param name="setter">The the function for setting the property's value in the given object</param>
        /// <returns>The property's metadata.</returns>
        public static PropertyMetadata Add<TObject, TValue>(
            string name, Func<TObject, TValue> getter, Action<TObject, TValue> setter)
            where TObject : class
        {
            var metadata = AddInternal(name, getter, setter);

            AddInternal(name + ".IsGettable", (TObject x) => getter != null, null);
            AddInternal(name + ".IsSettable", (TObject x) => setter != null, null);

            return metadata;
        }

        /// <summary>
        /// Gets the <see cref="Binding.PropertyMetadata"/> for the specified object type's named property.
        /// </summary>
        /// <param name="type">The type of the object containing the property.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns>The metadata for the specified property.</returns>
        /// <exception cref="AmbiguousMatchException">More than one property is found with the specified name.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
        public static PropertyMetadata Get(Type type, string name)
        {
            ICollection<PropertyMetadata> values;

            if (Registry.TryGetValues(type, out values))
            {
                var metadata = values.FirstOrDefault(x => x.Name == name);

                if (metadata != null)
                    return metadata;
            }

            PropertyInfo sourceInfo;
            try
            {
                sourceInfo = type.GetProperty(name);
            }
            catch (AmbiguousMatchException)
            {
                // Try disambiguating properties that are overridden using the "new" keyword.
                sourceInfo = type.GetProperty(name, BindingFlags.DeclaredOnly);
            }

            return AddInternal(sourceInfo);
        }

        /// <summary>
        /// Creates a notifying setter if possible and adds the property to the property registry.
        /// <para>This is called internally for properties which have been discovered by reflection.</para>
        /// </summary>
        /// <param name="property">The <see cref="System.Reflection.PropertyInfo"/> for the property.</param>
        /// <returns>The property's metadata.</returns>
        private static PropertyMetadata AddInternal(PropertyInfo property)
        {
            var name = property.Name;
            var getMethod = property.GetGetMethod();
            var setMethod = property.GetSetMethod();
            var ownerType = property.ReflectedType;

            var getter = getMethod == null ? (Func<object, object>)null : o => getMethod.Invoke(o, null);

            Action<object, object> setter;

            if (setMethod == null)
                setter = (Action<object, object>)null;
            else
            if (typeof(IObservableObject).IsAssignableFrom(ownerType))
            {
                setter = MakeObservableObjectNotifyingSetter(
                    name, 
                    getter, 
                    (o, value) => setMethod.Invoke(o, new[] { value }));
            }
            else
            {
                setter = MakeNotifyingSetter(name, getter, (o, value) => setMethod.Invoke(o, new[] { value }));
            }

            var metadata = new PropertyMetadata
            {
                Type = property.PropertyType,
                Name = name,
                Get = getter,
                Set = setter
            };

            Registry.Add(ownerType, metadata);

            return metadata;
        }

        /// <summary>
        /// Adds the named property, its getter, and its setter, if they exist, to the property registry.
        /// <para>This is a private method which is called by
        /// <see cref="Binding.PropertyRegistry.Add{TObject, TValue}"/> to add the passed property or one of its
        /// sub-properties without recursively creating sub-properties on the sub-properties.</para>
        /// </summary>
        /// <typeparam name="TObject">The type of the object containing the property.</typeparam>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <param name="getter">The the function for getting the property's value from the object that
        /// contains it.</param>
        /// <param name="setter">The the function for setting the property's value in the given object</param>
        /// <returns>The property's metadata.</returns>
        private static PropertyMetadata AddInternal<TObject, TValue>(
            string name, Func<TObject, TValue> getter, Action<TObject, TValue> setter)
            where TObject : class
        {
            Type type = typeof(TObject);
            ICollection<PropertyMetadata> values;

            if (Registry.TryGetValues(type, out values) &&
                values.FirstOrDefault(x => x.Name == name) != null)
            {
                var message = string.Format("The {1} for {0} has already been registered.", name, type.FullName);
                throw new ArgumentException(message);
            }

            var metadata = new PropertyMetadata
                {
                    Type = typeof(TValue),
                    Name = name,
                    Get = getter == null ? (Func<object, object>)null : o => getter((TObject)o),
                    Set = setter == null ? (Action<object, object>)null : MakeNotifyingSetter(name, getter, setter)
                };

            Registry.Add(typeof(TObject), metadata);

            return metadata;
        }

        /// <summary>
        /// If the object supports the <see cref="Binding.IObservableObject"/> interface, wrap the passed setter with
        /// logic for firing the PropertyChanged notification; otherwise, if the property has a getter, only call the
        /// setter if the value has actually changed.
        /// </summary>
        /// <typeparam name="TObject">The type of the object containing the property.</typeparam>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <param name="getter">The the function for getting the property's value from the object that
        /// contains it.</param>
        /// <param name="setter">The the function for setting the property's value in the given object</param>
        /// <returns>The wrapped setter.</returns>
        private static Action<object, object> MakeNotifyingSetter<TObject, TValue>(
            string name, Func<TObject, TValue> getter, Action<TObject, TValue> setter)
            where TObject : class
        {
            // If TObject supports IObservableObject, create a notifying setter.
            if (typeof(IObservableObject).IsAssignableFrom(typeof(TObject)))
            {
                return MakeObservableObjectNotifyingSetter(
                    name,
                    (IObservableObject o) => getter((TObject)o),
                    (IObservableObject o, TValue value) => setter((TObject)o, value));
            }

            // Otherwise, if there is a getter, create a setter that guards against setting the property to the same
            // value in case the underlying setter does not do so.
            //
            // NOTE:  If two un-gettable properties were to be bidirectionally bound to each other, it would result in
            //        infinite recursion.
            if (getter != null)
            {
                return (propertyOwner, value) =>
                {
                    var owner = (TObject)propertyOwner;
                    if (value.Equals(getter(owner)))
                    {
                        return;
                    }

                    setter(owner, (TValue)value);
                };
            }

            return (propertyOwner, value) => setter((TObject)propertyOwner, (TValue)value);
        }

        /// <summary>
        /// Makes a notifying setter for objects that support the <see cref="Binding.IObservableObject"/> interface.
        /// </summary>
        /// <typeparam name="TObject">The type of the object containing the property.</typeparam>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <param name="getter">The the function for getting the property's value from the object that
        /// contains it.</param>
        /// <param name="setter">The the function for setting the property's value in the given object</param>
        /// <returns>A notifying setter.</returns>
        private static Action<object, object> MakeObservableObjectNotifyingSetter<TObject, TValue>(
            string name, Func<TObject, TValue> getter, Action<TObject, TValue> setter)
            where TObject : class
        {
            // NOTE:  If two un-gettable properties were to be bidirectionally bound to each other, it would result in
            //        infinite recursion.
            if (getter == null)
            {
                return (propertyOwner, value) =>
                {
                    var owner = (IObservableObject)propertyOwner;

                    setter((TObject)propertyOwner, (TValue)value);
                    owner.OnPropertyChangedEvent(name);
                };
            }

            return (propertyOwner, value) =>
            {
                var owner = (TObject)propertyOwner;

                if (value.Equals(getter(owner)))
                {
                    return;
                }

                setter(owner, (TValue)value);
                ((IObservableObject)propertyOwner).OnPropertyChangedEvent(name);
            };
        }
    }
}