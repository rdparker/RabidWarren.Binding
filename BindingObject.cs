// -----------------------------------------------------------------------
//  <copyright file="BindingObject.cs" company="Ron Parker">
//   Copyright 2014 Ron Parker
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

    /// <summary>
    /// Provides property binding services for an object.
    /// </summary>
    public class BindingObject : ObservableObject
    {
        /// <summary>
        /// Mappings for this object's properties.
        /// </summary>
        private readonly Multimap<Tuple<INotifyPropertyChanged, string>, Tuple<INotifyPropertyChanged, string>>
            _mappings = new Multimap<Tuple<INotifyPropertyChanged, string>, Tuple<INotifyPropertyChanged, string>>();

        /// <summary>
        /// The source objects whose <see cref="PropertyChanged"/> event has been subscribed to.
        /// </summary>
        private readonly List<INotifyPropertyChanged> _sourceObjects = new List<INotifyPropertyChanged>();

        /// <summary>
        /// The target objects whose <see cref="PropertyChanged"/> event has been subscribed to.
        /// </summary>
        private readonly List<INotifyPropertyChanged> _targetObjects = new List<INotifyPropertyChanged>();

        /// <summary>
        /// The data context against which properties are bound.
        /// </summary>
        private ObservableObject _dataContext;

        /// <summary>
        /// Initializes static members of the <see cref="BindingObject"/> class.
        /// </summary>
        static BindingObject()
        {
            // Register all of the BindingConverter types which are defined in this assembly.  They will be used by 
            // SourcePropertyChangedHandler below.
            ConverterRegistry.RegisterAll(typeof(BindingObject).Assembly);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Binding.BindingObject"/> class.
        /// </summary>
        public BindingObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Binding.BindingObject"/> class.
        /// </summary>
        /// <param name="dataContext">The data context, which acts as a source for bound properties.</param>
        public BindingObject(ObservableObject dataContext)
        {
            _dataContext = dataContext;
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
            foreach (var target in _sourceObjects)
            {
                target.PropertyChanged -= TargetPropertyChangedHandler;
            }
        }

        /// <summary>
        /// Binds the specified target property to the specified source property of the data context.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="target">The target object.</param>
        /// <param name="property">The target property.</param>
        /// <param name="source">The source property.</param>
        public void Bind<TTarget>(TTarget target, string property, string source)
            where TTarget : INotifyPropertyChanged
        {
            Bind(target, property, _dataContext, source);
        }

        /// <summary>
        /// Binds the named target property of the target object to the named source property of the source object.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetProperty">The name of the target property.</param>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceProperty">The name of the source source property.</param>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="targetProperty"/> is not a registered property of <paramref name="targetObject"/>
        /// or
        /// <paramref name="sourceProperty"/> is not a registered property of <paramref name="sourceObject"/>
        /// or
        /// <paramref name="targetProperty"/> has already been bound.
        /// </exception>
        public void Bind<TTarget, TSource>(TTarget targetObject, string targetProperty, TSource sourceObject, string sourceProperty)
            where TTarget : INotifyPropertyChanged
            where TSource : INotifyPropertyChanged
        {
            // Check pre-conditions.
            var targetType = typeof(TTarget);
            if (PropertyRegistry.Get(targetType, targetProperty) == null)
            {
                var message = string.Format("{0} is not a registered property of the target object.", targetProperty);

                throw new ArgumentException(message, "targetProperty");
            }

            if (PropertyRegistry.Get(sourceObject.GetType(), sourceProperty) == null)
            {
                var message = string.Format("{0} is not a registered property of the source object.", sourceProperty);

                throw new ArgumentException(message, "sourceProperty");
            }

            var noMatch = default(KeyValuePair<Tuple<INotifyPropertyChanged, string>, Tuple<INotifyPropertyChanged, string>>);
            var mappedTarget = _mappings.FirstOrDefault(
                pair =>
                    object.ReferenceEquals(pair.Value.Item1, targetObject) &&
                    pair.Value.Item2 == targetProperty);

            if (!mappedTarget.Equals(noMatch))
            {
                var message = string.Format(
                    "{0} is already bound.  A target property can only be bound once.", targetProperty);

                throw new ArgumentException(message, "targetProperty");
            }

            // Make sure we have subscribed to the source's PropertyChanged notification.
            if (!_sourceObjects.Any(x => object.ReferenceEquals(x, sourceObject)))
            {
                sourceObject.PropertyChanged += SourcePropertyChangedHandler;
                _sourceObjects.Add(sourceObject);
            }

            // Make sure we have subscribed to the targets's PropertyChanged notification.
            if (!_targetObjects.Any(x => object.ReferenceEquals(x, targetObject)))
            {
                targetObject.PropertyChanged += TargetPropertyChangedHandler;
                _targetObjects.Add(targetObject);
            }

            // Create mappings between the source and target properties.
            var source = Tuple.Create((INotifyPropertyChanged)sourceObject, sourceProperty);
            var target = Tuple.Create((INotifyPropertyChanged)targetObject, targetProperty);

            _mappings.Add(source, target);
            _mappings.Add(target, source);

            // Set the initial value
            SourcePropertyChangedHandler(sourceObject, new PropertyChangedEventArgs(sourceProperty));
        }

        private static void ConvertToTarget(
            object target, PropertyMetadata targetInfo, PropertyMetadata sourceInfo, object value)
        {
            Type converterType;

            if (ConverterRegistry.Registry.TryGetValue(
                Tuple.Create(sourceInfo.Type, targetInfo.Type),
                out converterType))
            {
                var converter = (BindingConverter)Activator.CreateInstance(converterType);
                var targetValue = converter.ConvertTo(value, targetInfo.Type, null);

                if (!object.ReferenceEquals(targetValue, BindingConverter.NoValue))
                {
                    targetInfo.Set(target, targetValue);
                }
            }
        }

        private static void ConvertFromTarget(
            object target, PropertyMetadata targetInfo, PropertyMetadata sourceInfo, object value)
        {
            Type converterType;

            if (ConverterRegistry.Registry.TryGetValue(
                Tuple.Create(targetInfo.Type, sourceInfo.Type),
                out converterType))
            {
                var converter = (BindingConverter)Activator.CreateInstance(converterType);
                var targetValue = converter.ConvertFrom(value, targetInfo.Type, null);

                if (!object.ReferenceEquals(targetValue, BindingConverter.NoValue))
                {
                    targetInfo.Set(target, targetValue);
                }
            }
        }

        /// <summary>
        /// Handles <see cref="PropertyChanged"/> events for the from bound sources.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SourcePropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            ProcessPropertyChangedEvent(sender, e, ConvertToTarget);
        }

        /// <summary>
        /// Handles <see cref="PropertyChanged"/> events for the from bound targets.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TargetPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            ProcessPropertyChangedEvent(sender, e, ConvertFromTarget);
        }

        /// <summary>
        /// Copies a value between properties during a <see cref="PropertyChanged"/> event using the given
        /// conversion function.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event arguments.</param>
        /// <param name="convert">The conversion function.</param>
        private void ProcessPropertyChangedEvent(
            object sender,
            PropertyChangedEventArgs e, 
            Action<INotifyPropertyChanged, PropertyMetadata, PropertyMetadata, object> convert)
        {
            var source = Tuple.Create((INotifyPropertyChanged)sender, e.PropertyName);

            ICollection<Tuple<INotifyPropertyChanged, string>> values;

            if (_mappings.TryGetValues(source, out values))
            {
                foreach (var target in values)
                {
                    PropertyMetadata targetInfo = PropertyRegistry.Get(target.Item1.GetType(), target.Item2);
                    PropertyMetadata sourceInfo = PropertyRegistry.Get(source.Item1.GetType(), source.Item2);
                    var value = sourceInfo.Get(sender);

                    if (targetInfo.Set != null)
                    {
                        if (targetInfo.Type == sourceInfo.Type)
                        {
                            targetInfo.Set(target.Item1, value);
                        }
                        else
                        {
                            convert(target.Item1, targetInfo, sourceInfo, value);
                        }
                    }
                }
            }
        }
    }
}
