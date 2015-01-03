// -----------------------------------------------------------------------
//  <copyright file="Property.cs" company="Ron Parker">
//   Copyright 20142 2015 Ron Parker
//  </copyright>
//  <summary>
//   Maintains a registry of bindable properties definitions.  The property instances are tracked
//   within the individual bindable objects.
//  </summary>
// ----------------------------------------------------------------------- 
namespace Binding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using RabidWarren.Collections.Generic;

    /// ////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Maintains a registry of property definitions for binding.  The property instances are tracked
    /// within individual BindingObjects.
    /// </summary>
    ///
    /// <remarks>   Last edited by Ron, 12/24/2014. </remarks>
    /// ////////////////////////////////////////////////////////////////////////////////////////////////
    public static class Property
    {
        /// <summary>   Maps class names to lists of properties. </summary>
        static readonly Multimap<Type, PropertyMetadata> Registry = new Multimap<Type, PropertyMetadata>();

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Specifies the order of precedence when searching for properties via reflection.  NonPublic
        /// properties are included because Visual Studio creates WPF controls as private fields.
        /// </summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        static readonly BindingFlags[] VisibilityPrecedence =
        {
            BindingFlags.Static   | BindingFlags.Public,
            BindingFlags.Instance | BindingFlags.Public,
            BindingFlags.Static   | BindingFlags.NonPublic,
            BindingFlags.Instance | BindingFlags.NonPublic
        };

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
        public static PropertyMetadata Register<TValue>(Type type, string name, TValue value)
        {
            var metadata = new PropertyMetadata
            {
                Type = typeof(TValue),
                Name = name,
                Get = _ => value
            };

            Registry.Add(type, metadata);

            return metadata;
        }

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
        /// <param name="setter">   The function for setting the property on a given object. </param>
        ///
        /// <returns>   The property's metadata. </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public static PropertyMetadata Register<TObject, TValue>(string name, Action<TObject, TValue> setter)
            where TObject : class
        {
            return Register(name, null, setter);
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Registers the named property and it accessors for binding. </summary>
        ///
        /// <remarks>   Last edited by Ron, 12/24/2014. </remarks>
        ///
        /// <exception cref="ArgumentException">    Thrown if the property was already registered. </exception>
        ///
        /// <typeparam name="TObject">  The type of the object containing the property. </typeparam>
        /// <typeparam name="TValue">   The type of the property. </typeparam>
        /// <param name="name">     The name of the property. </param>
        /// <param name="getter">   The function for getting the named property from an object. </param>
        /// <param name="setter">   The function for setting the named property on an object. </param>
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
        /// Finds the PropertyMetadata for the given type's named property.
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
            PropertyMetadata metadata;
            ICollection<PropertyMetadata> values;

            // Try to lookup the property in the registry.
            if (Registry.TryGetValues(type, out values))
            {
                metadata = values.FirstOrDefault(x => x.Name == name);

                if (metadata != null)
                    return metadata;
            }

            // If it wasn't found in he registry, try locating it via reflection.
            metadata = FromPropertyInfo(type, name) ?? FromMethods(type, name) ?? FromField(type, name);
            if (metadata == null)
            {
                // Support the pseudo-properties, CanRead and CanWrite, similar to the same properties
                // of PropertyInfo.
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

                    metadata = new PropertyMetadata
                    {
                        Type = innerType,
                        Name = name,
                        Get = o => innerGet(outerGet(o))
                    };

                    metadata.Set = MakeNotifyingSetter(
                        name, 
                        metadata.Get, 
                        (o, value) => innerSet(outerGet(o), value));

                    Registry.Add(outerType, metadata);

                    return metadata;
                }
            }

            // If it was found, cache the discovered information in the registry.
            if (metadata != null)
                Registry.Add(type, metadata);

            return metadata;
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the value of a property using reflection. </summary>
        ///
        /// <remarks>   Last edited by Ron, 12/27/2014. </remarks>
        ///
        /// <param name="obj">  The object the property belongs to. </param>
        /// <param name="name"> The property's name. </param>
        ///
        /// <returns>   The property's value. </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public static object GetReflected(object obj, string name)
        {
            var get = GetReflectedGetMethod(obj.GetType(), name);
                
            return get(obj);
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets a property's get method using reflection. </summary>
        ///
        /// <remarks>   Last edited by Ron, 1/3/2015. </remarks>
        ///
        /// <param name="type"> The type of the object containing the property. </param>
        /// <param name="name"> The name of the property. </param>
        ///
        /// <returns>
        /// On success, the method or closure for getting the property; otherwise, <c>null</c>.
        /// </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public static Func<object, object> GetReflectedGetMethod(Type type, string name)
        {
            // Loop through visibility levels from most visible to least visible.
            foreach (var visibility in VisibilityPrecedence)
            {
                // Prefer properties first.
                PropertyInfo info = GetPropertyInfo(type, name, visibility);
                if (info != null)
                {
                    var getter = info.GetGetMethod(visibility.HasFlag(BindingFlags.NonPublic));
                    if (getter != null)
                        return o => getter.Invoke(o, null);
                }

                // Next come get methods.
                var getMethod = type.GetMethod("get_" + name, visibility);
                if (getMethod != null)
                    return o => getMethod.Invoke(o, null);

                // Finally, check for fields.
                var field = type.GetField(name, visibility);
                if (field != null)
                    return field.GetValue;
            }

            // No match was found.
            return null;
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// If the object supports the <see cref="Binding.INotifyingObject"/> interface, wrap the passed
        /// setter with logic for firing the PropertyChanged notification; otherwise, if the property has
        /// a getter, only call the setter when the value changes.
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
        static Action<object, object> MakeSmartSetter<TObject, TValue>(
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
        /// If the <paramref name="ownerType"/> supports the <see cref="INotifyingObject"/> interface,
        /// wrap the passed setter with logic for firing the PropertyChanged notification; otherwise, if
        /// the property has a getter, only call the setter when the value really changes.
        /// <para>
        /// This form is used when the owner type cannot be inferred from the parameters, but must be
        /// passed explicitly.  This occurs when the parameters are derived from
        /// <see cref="PropertyInfo"/> resulting in generic object types being used for the getter and
        /// setter.</para>
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 12/24/2014. </remarks>
        ///
        /// <param name="ownerType">    Type of the owner. </param>
        /// <param name="name">         The name of the property. </param>
        /// <param name="getter">       The function for getting the property's value from the object
        ///                             that contains it. </param>
        /// <param name="simpleSetter"> The unadorned function for setting the property's value on a
        ///                             given object. </param>
        ///
        /// <returns>   The wrapped setter. </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        static Action<object, object> MakeSmartSetter(
            Type ownerType, string name, Func<object, object> getter, Action<object, object> simpleSetter)
        {
            Action<object, object> smartSetter =
                (simpleSetter == null)
                    ? null :
                typeof(INotifyingObject).IsAssignableFrom(ownerType)
                    ? MakeNotifyingSetter(name, getter, simpleSetter)
                    : MakeSmartSetter(name, getter, simpleSetter);

            return smartSetter;
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Generates metadata by looking up PropertyInfo via reflection. </summary>
        ///
        /// <remarks>   Last edited by Ron, 1/3/2015. </remarks>
        ///
        /// <param name="type"> The type of the object containing the property. </param>
        /// <param name="name"> The name of the property. </param>
        ///
        /// <returns>
        /// The property metadata if property info was found by reflection; otherwise <c>null</c>.
        /// </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        static PropertyMetadata FromPropertyInfo(Type type, string name)
        {
            foreach (var visibility in VisibilityPrecedence)
            {
                PropertyInfo info = GetPropertyInfo(type, name, visibility);
                if (info != null)
                    return info.ToMetadata();
            }

            return null;
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Generates metadata by looking up <c>get_*</c> and <c>set_*</c> methods via reflection.
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 1/3/2015. </remarks>
        ///
        /// <param name="type"> The type of the object containing the property. </param>
        /// <param name="name"> The name of the property. </param>
        ///
        /// <returns>
        /// The property metadata if at least one of the methods was found via reflection; otherwise,
        /// <c>null</c>.
        /// </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        static PropertyMetadata FromMethods(Type type, string name)
        {
            var metadata = new PropertyMetadata
            {
                Name = name
            };

            // Find the get and set methods, if any.  These are looked up together because we never want to
            // return one that has less visibility than the other, such as a private setter with a public
            // getter.  The NonPublic one's are only looked up because Visual Studio creates WPF controls
            // as private fields. 
            foreach (var visibility in VisibilityPrecedence)
            {
                var getMethod = type.GetMethod("get_" + name, visibility);
                if (getMethod != null)
                {
                    metadata.Get = o => getMethod.Invoke(o, null);
                    metadata.Type = getMethod.ReturnType;
                }

                var setMethod = type.GetMethod("set_" + name, visibility);
                if (setMethod != null)
                {
                    metadata.Set = (o, value) => setMethod.Invoke(o, new[] { value });

                    // If there is no getter, get the type from the setter's parameters.
                    if (metadata.Type == null)
                    {
                        var parameters = setMethod.GetParameters();

                        // Whether this is a regular property or an indexer, the value type is always last. 
                        metadata.Type = parameters.Last().GetType();
                    }
                }

                if (getMethod != null || setMethod != null) break;
            }

            return metadata.Type == null ? null : metadata;
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Generates metadata by looking up fields via reflection. </summary>
        ///
        /// <remarks>   Last edited by Ron, 1/3/2015. </remarks>
        ///
        /// <param name="type"> The type of the object containing the property. </param>
        /// <param name="name"> The name of the property. </param>
        ///
        /// <returns>
        /// The property metadata if a matching field was found; otherwise, <c>null</c>.
        /// </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        static PropertyMetadata FromField(Type type, string name)
        {
            var metadata = new PropertyMetadata
            {
                Name = name
            };

            FieldInfo field;
            foreach (var visibility in VisibilityPrecedence)
            {
                field = type.GetField(name, visibility);

                if (field != null)
                {
                    return new PropertyMetadata
                    {
                        Type = field.FieldType,
                        Name = name,
                        Get = field.GetValue,
                        Set = MakeSmartSetter(type, name, field.GetValue, field.SetValue)
                    };
                }
            }

            return null;
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// A PropertyInfo extension method that converts between PropertyInfo and
        /// <see cref="PropertyMetadata"/>.
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 1/3/2015. </remarks>
        ///
        /// <param name="property"> The property to act on. </param>
        ///
        /// <returns>   property as a PropertyMetadata. </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        static PropertyMetadata ToMetadata(this PropertyInfo property)
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
            Action<object, object> simpleSetter = null;
            if (setMethod != null)
                simpleSetter = (o, value) => setMethod.Invoke(o, new[] { value });

            Action<object, object> setter = MakeSmartSetter(ownerType, name, getter, simpleSetter);

            return new PropertyMetadata
            {
                Type = property.PropertyType,
                Name = name,
                Get = getter,
                Set = setter
            };
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets PropertyInformation using reflection. </summary>
        ///
        /// <remarks>   Last edited by Ron, 1/3/2015. </remarks>
        ///
        /// <param name="type">         The type of the object containing the property. </param>
        /// <param name="name">         The name of the property. </param>
        /// <param name="visibility">   The visibility. </param>
        ///
        /// <returns>   The property information. </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        static PropertyInfo GetPropertyInfo(Type type, string name, BindingFlags visibility)
        {
            PropertyInfo info;
            try
            {
                info = type.GetProperty(name, visibility);
            }
            catch (AmbiguousMatchException)
            {
                // Try disambiguating properties that are overridden using the "new" keyword.
                info = type.GetProperty(name, visibility | BindingFlags.DeclaredOnly);
            }

            return info;
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Makes a notifying setter for objects that support the <see cref="INotifyingObject"/>
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
        static Action<object, object> MakeNotifyingSetter<TObject, TValue>(
            string name, Func<TObject, TValue> getter, Action<TObject, TValue> setter)
            where TObject : class
        {
            // NOTE:  If two un-gettable properties were to be bidirectionally bound to each other, it
            //        would result in infinite recursion. 
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