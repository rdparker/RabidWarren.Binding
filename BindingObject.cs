// -----------------------------------------------------------------------
//  <copyright file="BindingObject.cs" company="Ron Parker">
//   Copyright 2014, 2015 Ron Parker
//  </copyright>
//  <summary>
//   Provides property binding services for an object.
//  </summary>
// -----------------------------------------------------------------------

namespace Binding
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using RabidWarren.Collections.Generic;

    /// ////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary> Represents an object that binds its properties to other objects. </summary>
    ///
    /// <remarks>
    /// Due to the single inheritance nature of C#, it may be necessary for a class to include a
    /// BindingObject as a member, rather than deriving from the class itself.  As a convenience a
    /// NotifyingObject data context may be passed to the constructor, simplifying multiple calls to
    /// <see cref="Bind"/>.
    /// </remarks>
    /// ////////////////////////////////////////////////////////////////////////////////////////////////
    public class BindingObject : NotifyingObject
    {
        /// <summary>
        /// Bindings that this object has established to other object's properties.
        /// </summary>
        /// <remarks>
        /// The keys and values have the same type, <see cref="Tuple{INotifyPropertyChanged, string}"/>.  This contains
        /// two elements, the object that contains the property and the path from the object to the property.
        /// <para>
        /// A property path may be the name of a property which is directly owned by the object or a dotted path to a
        /// property.  For example, if an object has a Text property its path would be "Text", but if it has a TextBox
        /// control with a Text property, the path would be "TextBox.Text".
        /// </para></remarks>
        readonly Multimap<Tuple<INotifyPropertyChanged, string>, Tuple<INotifyPropertyChanged, string>>
            _bindings = new Multimap<Tuple<INotifyPropertyChanged, string>, Tuple<INotifyPropertyChanged, string>>();

        /// <summary>
        /// Tracks the source objects whose <see cref="PropertyChanged"/> event has been subscribed to.
        /// </summary>
        readonly List<INotifyPropertyChanged> _sourceObjects = new List<INotifyPropertyChanged>();

        /// <summary>
        /// Tracks the target objects whose <see cref="PropertyChanged"/> event has been subscribed to.
        /// <para>
        /// Normally this will just be the <see cref="BindingObject"/>, but if <see cref="Bind"/> is passed a property
        /// path which passes through another object that supports <see cref="INotifyPropertyChanged"/> the binding
        /// system may use it as a target.
        /// </para>
        /// </summary>
        readonly List<INotifyPropertyChanged> _targetObjects = new List<INotifyPropertyChanged>();

        /// <summary> The optional source object used by the short form of <see cref="Bind"/>. </summary>
        NotifyingObject _sourceObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="Binding.BindingObject"/> class.
        /// </summary>
        public BindingObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Binding.BindingObject"/> class.
        /// </summary>
        /// <param name="sourceObject">The object which acts as a source for bound properties.</param>
        public BindingObject(NotifyingObject sourceObject)
        {
            _sourceObject = sourceObject;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="BindingObject"/> class, unregistering it from all
        /// <see cref="PropertyChanged"/> events.
        /// </summary>
        ~BindingObject()
        {
            // Unsubscribe from all source property notifications
            foreach (var source in _sourceObjects)
            {
                source.PropertyChanged -= SourcePropertyChangedHandler;
            }

            // Unsubscribe from all target property notifications
            foreach (var target in _targetObjects)
            {
                target.PropertyChanged -= TargetPropertyChangedHandler;
            }
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Binds the specified target property to the specified source property using the source object
        /// that was passed to <see cref="M:BindingObject(NotifyingObject)"/>.
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 1/3/2015. </remarks>
        ///
        /// <typeparam name="TTarget">  The type of the target. </typeparam>
        /// <param name="target">   The target object. </param>
        /// <param name="property"> The target property. </param>
        /// <param name="source">   The source property. </param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public void Bind<TTarget>(TTarget target, string property, string source)
            where TTarget : INotifyPropertyChanged
        {
            Bind(target, property, _sourceObject, source);
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Binds the target object's named property to the source object's named property.
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 12/24/2014. </remarks>
        ///
        /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
        ///                                         illegal values. </exception>
        ///
        /// <typeparam name="TTarget">  The type of the target object. </typeparam>
        /// <typeparam name="TSource">  The type of the source object. </typeparam>
        /// <param name="targetObject">     The target object. </param>
        /// <param name="targetProperty">   The name of the target property. </param>
        /// <param name="sourceObject">     The source object. </param>
        /// <param name="sourceProperty">   The name of the source source property. </param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public void Bind<TTarget, TSource>(TTarget targetObject, string targetProperty, TSource sourceObject, string sourceProperty)
            where TTarget : INotifyPropertyChanged
            where TSource : INotifyPropertyChanged
        {
            var targetNotifiable = (INotifyPropertyChanged)targetObject;
            var sourceNotifiable = (INotifyPropertyChanged)sourceObject;
            PropertyPath.FindLeafNotifiable(ref targetNotifiable, ref targetProperty);
            PropertyPath.FindLeafNotifiable(ref sourceNotifiable, ref sourceProperty);

            // Check pre-conditions.
            var targetType = targetNotifiable.GetType();
            if (Property.Find(targetType, targetProperty) == null)
                throw new ArgumentException("Cannot bind to an unregistered property.", "targetProperty");

            if (Property.Find(sourceNotifiable.GetType(), sourceProperty) == null)
                throw new ArgumentException("Cannot bind to an unregistered property.", "sourceProperty");

            var noMatch = default(KeyValuePair<Tuple<INotifyPropertyChanged, string>, Tuple<INotifyPropertyChanged, string>>);
            var mappedTarget = _bindings.FirstOrDefault(
                pair =>
                    ReferenceEquals(pair.Value.Item1, targetNotifiable) &&
                    pair.Value.Item2 == targetProperty);

            if (!mappedTarget.Equals(noMatch))
                throw new ArgumentException("A target property can only be bound once.", "targetProperty");

            // Make sure we have subscribed to the source's PropertyChanged notification.
            if (!_sourceObjects.Any(x => ReferenceEquals(x, sourceNotifiable)))
            {
                sourceNotifiable.PropertyChanged += SourcePropertyChangedHandler;
                _sourceObjects.Add(sourceNotifiable);
            }

            // Make sure we have subscribed to the targets's PropertyChanged notification.
            if (!_targetObjects.Any(x => ReferenceEquals(x, targetNotifiable)))
            {
                targetNotifiable.PropertyChanged += TargetPropertyChangedHandler;
                _targetObjects.Add(targetNotifiable);
            }

            // Create mappings between the source and target properties.
            var source = Tuple.Create(sourceNotifiable, sourceProperty);
            var target = Tuple.Create(targetNotifiable, targetProperty);

            _bindings.Add(source, target);
            _bindings.Add(target, source);

            // Set the initial value
            //
            // Note: If more than one target is bound to the same source, some target's will receive multiple
            //       notifications.
            SourcePropertyChangedHandler(sourceNotifiable, new PropertyChangedEventArgs(sourceProperty));
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Updates the binding target property by converting the source value to the target's type.
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 1/3/2015. </remarks>
        ///
        /// <param name="owner">        The target property owner. </param>
        /// <param name="targetInfo">   Information describing the target property. </param>
        /// <param name="sourceType">   The type of the source property. </param>
        /// <param name="value">        The current source property value. </param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        static void UpdateTarget(object owner, PropertyMetadata targetInfo, Type sourceType, object value)
        {
            var converterType = ConverterRegistry.Find(sourceType, targetInfo.Type);

            if (converterType != null)
            {
                var converter = (BindingConverter)Activator.CreateInstance(converterType);
                var targetValue = converter.ConvertTo(value, targetInfo.Type, null);

                if (!ReferenceEquals(targetValue, BindingConverter.NoValue))
                {
                    targetInfo.Set(owner, targetValue);
                }
            }
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Updates the binding source property from the target's value using the appropriate binding
        /// converter.
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 1/3/2015. </remarks>
        ///
        /// <param name="owner">        The source property owner. </param>
        /// <param name="sourceInfo">   Information describing the source. </param>
        /// <param name="targetType">   The type of the binding target property. </param>
        /// <param name="value">        The current target property value. </param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        static void UpdateSource(object owner, PropertyMetadata sourceInfo, Type targetType, object value)
        {
            var converterType = ConverterRegistry.Find(sourceInfo.Type, targetType);

            if (converterType != null)
            {
                var converter = (BindingConverter)Activator.CreateInstance(converterType);
                var sourceValue = converter.ConvertFrom(value, sourceInfo.Type, null);

                if (!ReferenceEquals(sourceValue, BindingConverter.NoValue))
                {
                    sourceInfo.Set(owner, sourceValue);
                }
            }
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Handles <see cref="PropertyChanged"/> events from bound sources. </summary>
        ///
        /// <remarks>   Last edited by Ron, 1/3/2015. </remarks>
        ///
        /// <param name="sender">   The sender. </param>
        /// <param name="e">        The event arguments. </param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        void SourcePropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            ProcessPropertyChangedEvent(sender, e, UpdateTarget);
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Handles <see cref="PropertyChanged"/> events from bound targets. </summary>
        ///
        /// <remarks>   Last edited by Ron, 1/3/2015. </remarks>
        ///
        /// <param name="sender">   The sender. </param>
        /// <param name="e">        The event arguments. </param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        void TargetPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            ProcessPropertyChangedEvent(sender, e, UpdateSource);
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Copies a value between properties during a property changed event using the given conversion
        /// function.
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 12/24/2014. </remarks>
        ///
        /// <param name="sender">   The sender. </param>
        /// <param name="e">        The event arguments. </param>
        /// <param name="convert">  The conversion function. </param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        void ProcessPropertyChangedEvent(
            object sender,
            PropertyChangedEventArgs e,
            Action<INotifyPropertyChanged, PropertyMetadata, Type, object> convert)
        {
            var source = Tuple.Create((INotifyPropertyChanged)sender, e.PropertyName);

            ICollection<Tuple<INotifyPropertyChanged, string>> values;

            if (_bindings.TryGetValues(source, out values))
            {
                foreach (var target in values)
                {
                    PropertyMetadata targetInfo = Property.Find(target.Item1.GetType(), target.Item2);
                    PropertyMetadata sourceInfo = Property.Find(source.Item1.GetType(), source.Item2);
                    var value = sourceInfo.Get(sender);

                    if (targetInfo.Set != null)
                    {
                        if (targetInfo.Type == sourceInfo.Type)
                        {
                            targetInfo.Set(target.Item1, value);
                        }
                        else
                        {
                            convert(target.Item1, targetInfo, sourceInfo.Type, value);
                        }
                    }
                }
            }
        }
    }
}
