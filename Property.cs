﻿// -----------------------------------------------------------------------
//  <copyright file="Property.cs" company="Ron Parker">
//   Copyright 2014, 2015 Ron Parker
//  </copyright>
//  <summary>
//   Maintains a registry of bindable properties definitions.  The property instances are tracked
//   within the individual bindable objects.
//  </summary>
// ----------------------------------------------------------------------- 

namespace RabidWarren.Binding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Collections.Generic;

    /// <summary>
    ///     Maintains a registry of property definitions for binding.  The property instances are tracked within
    ///     individual BindingObjects.
    /// </summary>
    public static class Property
    {
        /// <summary>
        ///     Specifies the order of precedence when searching for properties via reflection.  NonPublic properties
        ///     are included because Visual Studio creates WPF controls as private fields.
        /// </summary>
        static readonly BindingFlags[] VisibilityPrecedence =
        {
            BindingFlags.Static | BindingFlags.Public,
            BindingFlags.Instance | BindingFlags.Public,
            BindingFlags.Static | BindingFlags.NonPublic,
            BindingFlags.Instance | BindingFlags.NonPublic
        };

        /// <summary>
        ///     Finds the PropertyMetadata for the given type's named property.
        /// </summary>
        /// <param name="type"> The type of the object containing the property. </param>
        /// <param name="name"> The name of the property. </param>
        /// <returns>   The property's metadata if the property exists; otherwise, <c>null</c>. </returns>
        public static PropertyMetadata Find(Type type, string name)
        {
            // Try to lookup the property in the registry.
            var metadata = FindInRegistry(type, name);
            if (metadata != null)
                return metadata;

            // If it wasn't found in he registry, try locating it via reflection.
            metadata = FromPropertyInfo(type, name) ?? FromMethods(type, name) ?? FromField(type, name);
            if (metadata == null)
            {
                // Support the pseudo-properties, CanRead and CanWrite, similar to the same properties
                // of PropertyInfo.
                var namePath = name.Split('.');
                if (namePath.Length == 2)
                {
                    if (namePath[1] == "CanRead")
                        return Register(type, name, Find(type, namePath[0]).Get != null);

                    if (namePath[1] == "CanWrite")
                        return Register(type, name, Find(type, namePath[0]).Set != null);
                }

                if (namePath.Length >= 2)
                {
                    var outer = Find(type, namePath[0]);
                    var innerProperty = string.Join(".", namePath.Skip(1));
                    var outerGet = outer.Get;
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

                    AddToRegistry(type, metadata);

                    return metadata;
                }
            }

            // If it was found, cache the discovered information in the registry.
            if (metadata != null)
                AddToRegistry(type, metadata);

            return metadata;
        }

        /// <summary>   Gets a property's get method using reflection. </summary>
        /// <param name="type"> The type of the object containing the property. </param>
        /// <param name="name"> The name of the property. </param>
        /// <returns>
        ///     On success, the method or closure for getting the property; otherwise, <c>null</c>.
        /// </returns>
        static Func<object, object> GetReflectedGetMethod(Type type, string name)
        {
            // Loop through visibility levels from most visible to least visible.
            foreach (var level in VisibilityPrecedence)
            {
                // Prefer properties first.
                var getter = type.GetProperty(name, level)?.GetGetMethod(level.HasFlag(BindingFlags.NonPublic));
                if (getter != null)
                    return o => getter.Invoke(o, null);

                // Next come get methods.
                var getMethod = type.GetMethod("get_" + name, level);
                if (getMethod != null)
                    return o => getMethod.Invoke(o, null);

                // Finally, check for fields.
                var field = type.GetField(name, level);
                if (field != null)
                    return field.GetValue;
            }

            // No match was found.
            return null;
        }

        /// <summary>   Gets the value of a property using reflection. </summary>
        /// <exception cref="ArgumentException">   Thrown when name does not specify a property.</exception>
        /// <param name="obj">  The object the property belongs to. </param>
        /// <param name="name"> The property's name. </param>
        /// <returns>   The property's value. </returns>
        internal static object GetReflected(object obj, string name)
        {
            var get = GetReflectedGetMethod(obj.GetType(), name);

            if (get == null)
                throw new ArgumentException("Nonexistent property", nameof(name));

            return get(obj);
        }

        /// <summary>
        ///     Registers the named property and a getter that returns a constant value for binding.
        /// </summary>
        /// <typeparam name="TValue">   Type of the value. </typeparam>
        /// <param name="type">     The type of the object containing the property. </param>
        /// <param name="name">     The name of the property. </param>
        /// <param name="value">    The value to be returned from the getter. </param>
        /// <returns>   The property metadata. </returns>
        internal static PropertyMetadata Register<TValue>(Type type, string name, TValue value)
        {
            var metadata = new PropertyMetadata
            {
                Type = typeof (TValue),
                Name = name,
                Get = _ => value
            };

            AddToRegistry(type, metadata);

            return metadata;
        }

        /// <summary>
        ///     If the <paramref name="ownerType" /> supports the <see cref="INotifyingObject" /> interface, wrap the
        ///     passed setter with logic for firing the PropertyChanged notification; otherwise, if the property has a
        ///     getter, only call the setter when the value really changes.
        ///     <para>
        ///         This form is used when the owner type cannot be inferred from the parameters, but must be passed
        ///         explicitly.  This occurs when the parameters are derived from <see cref="PropertyInfo" /> resulting
        ///         in generic object types being used for the getter and setter.
        ///     </para>
        /// </summary>
        /// <param name="ownerType">    Type of the owner. </param>
        /// <param name="name">         The name of the property. </param>
        /// <param name="getter">
        ///     The function for getting the property's value from the object that contains it.
        /// </param>
        /// <param name="setter">
        ///     The unadorned function for setting the property's value on a given object.
        /// </param>
        /// <returns>   The wrapped setter. </returns>
        static Action<object, object> MakeSmartSetter(
            Type ownerType, string name, Func<object, object> getter, Action<object, object> setter)
        {
            if (typeof (INotifyingObject).IsAssignableFrom(ownerType))
                return MakeNotifyingSetter(name, getter, setter);

            // Otherwise, create a setter that guards against setting the property to the same value in case the
            // underlying setter does not do so.
            return (owner, value) =>
            {
                var oldValue = getter(owner);
                if (!value?.Equals(oldValue) ?? (oldValue != null))
                    setter(owner, value);
            };
        }

        /// <summary>   Generates metadata by looking up PropertyInfo via reflection. </summary>
        /// <param name="type"> The type of the object containing the property. </param>
        /// <param name="name"> The name of the property. </param>
        /// <returns> The property metadata if property info was found by reflection; otherwise <c>null</c>. </returns>
        static PropertyMetadata FromPropertyInfo(Type type, string name)
        {
            return (from visibility in VisibilityPrecedence
                select type.GetProperty(name, visibility)
                into info
                where info != null
                select info.ToMetadata()).FirstOrDefault();
        }

        /// <summary>
        ///     Generates metadata by looking up <c>get_*</c> and <c>set_*</c> methods via reflection.
        /// </summary>
        /// <param name="type"> The type of the object containing the property. </param>
        /// <param name="name"> The name of the property. </param>
        /// <returns>
        ///     The property metadata if at least one of the methods was found via reflection; otherwise, <c>null</c>.
        /// </returns>
        static PropertyMetadata FromMethods(Type type, string name)
        {
            var metadata = new PropertyMetadata
            {
                Name = name
            };

            // Find the get and set methods, if any.  These are looked up together because we never want to return one
            // that has less visibility than the other, such as a private setter with a public getter.  The NonPublic
            // one's are only looked up because Visual Studio creates WPF controls as private fields. 
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
                    metadata.Set = (o, value) => setMethod.Invoke(o, new[] {value});

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

        /// <summary>   Generates metadata by looking up fields via reflection. </summary>
        /// <param name="type"> The type of the object containing the property. </param>
        /// <param name="name"> The name of the property. </param>
        /// <returns> The property metadata if a matching field was found; otherwise, <c>null</c>. </returns>
        static PropertyMetadata FromField(Type type, string name)
        {
            return (from visibility in VisibilityPrecedence
                select type.GetField(name, visibility)
                into field
                where field != null
                select new PropertyMetadata
                {
                    Type = field.FieldType,
                    Name = name,
                    Get = field.GetValue,
                    Set = MakeSmartSetter(type, name, field.GetValue, field.SetValue)
                }).FirstOrDefault();
        }

        /// <summary>
        ///     A PropertyInfo extension method that converts between PropertyInfo and
        ///     <see cref="PropertyMetadata" />.
        /// </summary>
        /// <param name="property"> The property to act on. </param>
        /// <returns>   property as a PropertyMetadata. </returns>
        static PropertyMetadata ToMetadata(this PropertyInfo property)
        {
            var name = property.Name;
            var getMethod = property.GetGetMethod();
            var setMethod = property.GetSetMethod();
            var ownerType = property.ReflectedType;

            // Create a PropertyMetadata.Get function from the get method, if any.
            var getter =
                getMethod == null
                    ? (Func<object, object>) null
                    : o => getMethod.Invoke(o, null);

            // Create a PropertyMetadata.Set function from the set method, if any.
            Action<object, object> simpleSetter = null;
            if (setMethod != null)
                simpleSetter = (o, value) => setMethod.Invoke(o, new[] {value});

            var setter =
                getter == null
                    ? simpleSetter
                    : simpleSetter == null ? null : MakeSmartSetter(ownerType, name, getter, simpleSetter);

            return new PropertyMetadata
            {
                Type = property.PropertyType,
                Name = name,
                Get = getter,
                Set = setter
            };
        }

        /// <summary>
        ///     Makes a notifying setter for objects that support the <see cref="INotifyingObject" /> interface.
        /// </summary>
        /// <typeparam name="TObject">  The type of the object containing the property. </typeparam>
        /// <typeparam name="TValue">   The type of the property value. </typeparam>
        /// <param name="name">     The name of the property. </param>
        /// <param name="getter">
        ///     The the function for getting the property's value from the object that contains it.
        /// </param>
        /// <param name="setter">   The the function for setting the property's value in the given object. </param>
        /// <returns>   A notifying setter. </returns>
        static Action<object, object> MakeNotifyingSetter<TObject, TValue>(
            string name, Func<TObject, TValue> getter, Action<TObject, TValue> setter)
            where TObject : class
            where TValue : class
        {
            return (propertyOwner, value) =>
            {
                var owner = (TObject) propertyOwner;

                var oldValue = getter(owner);
                if (value == null)
                {
                    if (oldValue == null)
                        return;
                }
                else if (value.Equals(getter(owner)))
                {
                    return;
                }

                setter(owner, (TValue) value);
#pragma warning disable 0618
                ((INotifyingObject) propertyOwner).OnPropertyChangedEvent(name);
#pragma warning restore 0618
            };
        }

        #region PropertyRegistry

        // Controls access to the property registry, enforcing singular registration of a property.

        /// <summary>   Maps class names to lists of properties. </summary>
        static readonly Multimap<Type, PropertyMetadata> Registry = new Multimap<Type, PropertyMetadata>();

        /// <summary>
        ///     Adds a property entry using the given type and property metadata.
        /// </summary>
        /// <exception cref="ArgumentException">
        ///     Thrown when the given property has already been registered.
        /// </exception>
        /// <param name="type">The type of the object containing the property.</param>
        /// <param name="metadata">The metadata that defines the property.</param>
        static void AddToRegistry(Type type, PropertyMetadata metadata)
        {
            var name = metadata.Name;

            if (Contains(type, name))
                throw new ArgumentException($"The {type.FullName}.{name} property has already been registered.");

            Registry.Add(type, metadata);
        }

        /// <summary>
        ///     Finds the given registered property, if any.
        /// </summary>
        /// <param name="type">The type containing the property.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns>The metadata describing the property.</returns>
        static PropertyMetadata FindInRegistry(Type type, string name)
        {
            ICollection<PropertyMetadata> values;

            return Registry.TryGetValues(type, out values) ? values.FirstOrDefault(x => x.Name == name) : null;
        }

        /// <summary>
        ///     Checks if the given property exists in the registry.
        /// </summary>
        /// <param name="type">The type containing the property.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns><c>true</c> if the property has been registered; otherwise <c>false.</c>.</returns>
        static bool Contains(Type type, string name)
        {
            return FindInRegistry(type, name) != null;
        }

        #endregion
    }
}