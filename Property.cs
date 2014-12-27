// -----------------------------------------------------------------------
//  <copyright file="Property.cs" company="Ron Parker">
//   Copyright 2014 Ron Parker
//  </copyright>
//  <summary>
//   Implements bindable property methods and a central registry for them.
//  </summary>
// -----------------------------------------------------------------------
namespace Binding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RabidWarren.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;

    /// ////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Implements the central registry for properties that may act as either the source or target of
    /// a binding.
    /// </summary>
    ///
    /// <remarks>   Last edited by Ron, 12/24/2014. </remarks>
    /// ////////////////////////////////////////////////////////////////////////////////////////////////
    public static class Property
    {
        /// <summary>   Maps class names to lists of properties. </summary>
        private static readonly Multimap<Type, PropertyMetadata> Registry = new Multimap<Type, PropertyMetadata>();

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Registers the named property and its setter for binding. </summary>
        ///
        /// <remarks>   Last edited by Ron, 12/24/2014. </remarks>
        ///
        /// <exception cref="ArgumentException">    Passed when the property has already been registered. </exception>
        ///
        /// <typeparam name="TObject">  The type of the object containing the property. </typeparam>
        /// <typeparam name="TValue">   The type of the property. </typeparam>
        /// <param name="name">     The name of the property. </param>
        /// <param name="setter">   The the function for setting the property on the given object. </param>
        ///
        /// <returns>   The property's metadata. </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public static PropertyMetadata Register<TObject, TValue>(string name, Action<TObject, TValue> setter)
            where TObject : class
        {
            return Register(name, null, setter);
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Registers the named property and it accessors for binding usage. </summary>
        ///
        /// <remarks>   Last edited by Ron, 12/24/2014. </remarks>
        ///
        /// <exception cref="ArgumentException">    Thrown if the property was already registered. </exception>
        ///
        /// <typeparam name="TObject">  The type of the object containing the property. </typeparam>
        /// <typeparam name="TValue">   The type of the property. </typeparam>
        /// <param name="name">     The name of the property. </param>
        /// <param name="getter">   The the function for getting the named property from an object. </param>
        /// <param name="setter">   The the function for setting the named property on an object. </param>
        ///
        /// <returns>   The property's metadata. </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public static PropertyMetadata Register<TObject, TValue>(
            string name, Func<TObject, TValue> getter = null, Action<TObject, TValue> setter = null)
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
                Set = setter == null ? (Action<object, object>)null : MakeSmartSetter(name, getter, setter)
            };

            Registry.Add(type, metadata);

            return metadata;
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Finds the <see cref="Binding.PropertyMetadata"/> for the specified property of the given type.
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 12/24/2014. </remarks>
        ///
        /// <param name="type"> The type of the object containing the property. </param>
        /// <param name="name"> The name of the property. </param>
        ///
        /// <returns>   The property's metadata if the property exists; otherwise, <c>null</c>. </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public static PropertyMetadata Find(Type type, string name)
        {
            PropertyMetadata metadata = null;
            ICollection<PropertyMetadata> values;

            // Try to lookup the property in the registry.
            if (Registry.TryGetValues(type, out values))
            {
                metadata = values.FirstOrDefault(x => x.Name == name);

                if (metadata != null)
                    return metadata;
            }

            // If it wasn't found in he registry, try locating it via reflection.
            System.Reflection.PropertyInfo sourceInfo;
            try
            {
                sourceInfo = type.GetProperty(name);
            }
            catch (System.Reflection.AmbiguousMatchException)
            {
                // Try disambiguating properties that are overridden using the "new" keyword.
                sourceInfo = type.GetProperty(name, System.Reflection.BindingFlags.DeclaredOnly);
            }

            // Support the pseudo-properties, CanRead and CanWrite, similar to the same properties
            // of PropertyInfo.
            if (sourceInfo == null)
            {
                var elts = name.Split('.');
                if (elts.Length == 2)
                {
                    if (elts[1] == "CanRead")
                        return Register(type, name, Find(type, elts[0]).Get != null);

                    if (elts[1] == "CanWrite")
                        return Register(type, name, Find(type, elts[0]).Set != null);
                }
                if (elts.Length >= 2)
                {
                    var outer = Find(type, elts[0]);
                    var innerProperty = string.Join(".", elts.Skip(1));
                    var outerGet = outer.Get;   // MainWindow -> TextBox
                    var outerType = outer.Type;

                    var inner = Find(outerType, innerProperty);
                    var innerGet = inner.Get;
                    var innerSet = inner.Set;
                    var innerType = inner.Type;

                    // TODO: Use a notifying setter and check invalid number field on focus loss.
                    //       Also remove other explicitly registered properties.
                    metadata = new PropertyMetadata {
                        Type = innerType,
                        Name = name,
                        Get = (o) => innerGet(outerGet(o)),
                        Set = (o, value) => innerSet(outerGet(o), value)
                    };
                    Registry.Add(outerType, metadata);

                    return metadata;
                }
            }

            // If it was found, cache the discovered information in the registry; otherwise, return null. 
            return sourceInfo == null ? null : AddInternal(sourceInfo);
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Registers the named property and a getter that returns a constant value for binding.
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 12/27/2014. </remarks>
        ///
        /// <typeparam name="TValue">   Type of the value. </typeparam>
        /// <param name="type">     The type of the object containing the property. </param>
        /// <param name="name">     The name of the property. </param>
        /// <param name="value">    The value to be returned from the getter. </param>
        ///
        /// <returns>   The property metadata. </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        private static PropertyMetadata Register<TValue>(Type type, string name, TValue value)
        {
            var metadata = new PropertyMetadata
            {
                Type = typeof(TValue),
                Name = name,
                Get = (_) => value
            };

            Registry.Add(type, metadata);

            return metadata;
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates a notifying setter if possible and adds the property to the property registry.
        /// <para>This is called internally for properties which have been discovered by
        /// reflection.</para>
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 12/24/2014. </remarks>
        ///
        /// <param name="property"> The <see cref="System.Reflection.PropertyInfo"/> for the property. </param>
        ///
        /// <returns>   The property's metadata. </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        private static PropertyMetadata AddInternal(System.Reflection.PropertyInfo property)
        {
            var name = property.Name;
            var getMethod = property.GetGetMethod();
            var setMethod = property.GetSetMethod();
            var ownerType = property.ReflectedType;

            // Create a PropertyMetadata.Get function from the get method, if any.
            var getter =
                getMethod == null ?
                (Func<object, object>)null : o => getMethod.Invoke(o, null);

            // Create a PropertyMetadata.Set function from the set method, if any.
            //
            // Note: MakeSmartSetter will normally call MakeNotifyingSetter when needed, but it cannot do that in this
            //       case.  It checks whether or not the inferred TObject type of the getter and setter is assignable
            //       to INotifyingObject, which in other usages, it is.  However, in this case TObject is just a
            //       generic System.Object, which is not assignable to INotifyingObject. So, that logic is duplicated
            //       here where the actual type can be checked.
            Action<object, object> simpleSetter = (o, value) => setMethod.Invoke(o, new[] { value });
            Action<object, object> smartSetter =
                (setMethod == null)
                    ? smartSetter = null :
                (typeof(INotifyingObject).IsAssignableFrom(ownerType))
                    ? MakeNotifyingSetter(name, getter, simpleSetter)
                    : MakeSmartSetter(name, getter, simpleSetter);

            var metadata = new PropertyMetadata
            {
                Type = property.PropertyType,
                Name = name,
                Get = getter,
                Set = smartSetter
            };

            Registry.Add(ownerType, metadata);

            return metadata;
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// If the object supports the <see cref="Binding.INotifyingObject"/> interface, wrap the passed
        /// setter with logic for firing the PropertyChanged notification; otherwise, if the property has
        /// a getter, only call the setter if the value has actually changed.
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 12/24/2014. </remarks>
        ///
        /// <typeparam name="TObject">  The type of the object containing the property. </typeparam>
        /// <typeparam name="TValue">   The type of the property value. </typeparam>
        /// <param name="name">     The name of the property. </param>
        /// <param name="getter">   The the function for getting the property's value from the object
        ///                         that contains it. </param>
        /// <param name="setter">   The the function for setting the property's value in the given
        ///                         object. </param>
        ///
        /// <returns>   The wrapped setter. </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        private static Action<object, object> MakeSmartSetter<TObject, TValue>(
            string name, Func<TObject, TValue> getter, Action<TObject, TValue> setter)
            where TObject : class
        {
            // If TObject supports INotifyingObject, create a notifying setter.
            if (typeof(INotifyingObject).IsAssignableFrom(typeof(TObject)))
            {
                return MakeNotifyingSetter(
                    name,
                    (INotifyingObject o) => getter((TObject)o),
                    (INotifyingObject o, TValue value) => setter((TObject)o, value));
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

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Makes a notifying setter for objects that support the <see cref="Binding.INotifyingObject"/>
        /// interface.
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 12/24/2014. </remarks>
        ///
        /// <typeparam name="TObject">  The type of the object containing the property. </typeparam>
        /// <typeparam name="TValue">   The type of the property value. </typeparam>
        /// <param name="name">     The name of the property. </param>
        /// <param name="getter">   The the function for getting the property's value from the object
        ///                         that contains it. </param>
        /// <param name="setter">   The the function for setting the property's value in the given
        ///                         object. </param>
        ///
        /// <returns>   A notifying setter. </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        private static Action<object, object> MakeNotifyingSetter<TObject, TValue>(
            string name, Func<TObject, TValue> getter, Action<TObject, TValue> setter)
            where TObject : class
        {
            // NOTE:  If two un-gettable properties were to be bidirectionally bound to each other, it would result in
            //        infinite recursion.
            if (getter == null)
            {
                return (propertyOwner, value) =>
                {
                    var owner = (INotifyingObject)propertyOwner;

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
                ((INotifyingObject)propertyOwner).OnPropertyChangedEvent(name);
            };
        }
    }
}