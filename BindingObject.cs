// -----------------------------------------------------------------------
//  <copyright file="BindingObject.cs" company="Ron Parker">
//   Copyright 2014, 2015 Ron Parker
//  </copyright>
//  <summary>
//   Provides property binding services for an object.
//  </summary>
// -----------------------------------------------------------------------

namespace RabidWarren.Binding
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using Collections.Generic;

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
        /// The keys and values have the same type, <see cref="T:Tuple{INotifyPropertyChanged, string}"/>.  This
        /// contains two elements, the object that contains the property and the path from the object to the property.
        /// <para>
        /// A property path may be the name of a property which is directly owned by the object or a dotted path to a
        /// property.  For example, if an object has a Text property its path would be "Text", but if it has a TextBox
        /// control with a Text property, the path would be "TextBox.Text".
        /// </para></remarks>
        readonly Multimap<Tuple<INotifyPropertyChanged, string>, Tuple<INotifyPropertyChanged, string>>
            _bindings = new Multimap<Tuple<INotifyPropertyChanged, string>, Tuple<INotifyPropertyChanged, string>>();

        /// <summary>
        /// Tracks the source objects whose <see cref="E:PropertyChanged"/> event has been subscribed to.
        /// </summary>
        readonly List<INotifyPropertyChanged> _sourceObjects = new List<INotifyPropertyChanged>();

        /// <summary>
        /// Tracks the target objects whose <see cref="E:PropertyChanged"/> event has been subscribed to.
        /// <para>
        /// Normally this will just be the <see cref="BindingObject"/>, but if <see cref="Bind"/> is passed a property
        /// path which passes through another object that supports <see cref="INotifyPropertyChanged"/> the binding
        /// system may use it as a target.
        /// </para>
        /// </summary>
        readonly List<INotifyPropertyChanged> _targetObjects = new List<INotifyPropertyChanged>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingObject"/> class.
        /// </summary>
        public BindingObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingObject"/> class.
        /// </summary>
        /// <param name="sourceObject">The object which acts as a source for bound properties.</param>
        public BindingObject(NotifyingObject sourceObject)
        {
            SourceObject = sourceObject;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="BindingObject"/> class, unregistering it from all
        /// <see cref="E:PropertyChanged"/> events.
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

        /// <summary>
        /// Gets the source object which is used by <see cref="M:Bind"/> calls which are not passed one.
        /// </summary>
        public NotifyingObject SourceObject { get; private set; }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Binds the specified target property of this instance to the specified source property using the
        /// source object that was passed to <see cref="M:BindingObject(NotifyingObject)"/>.
        /// </summary>
        ///
        /// <param name="target">   The target property. </param>
        /// <param name="source">   The source property. </param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public void Bind(string target, string source)
        {
            Bind(this, target, SourceObject, source);
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Binds the specified target property to the specified source's source property.
        /// </summary>
        ///
        /// <typeparam name="TSource">      The type of the source. </typeparam>
        /// <param name="property">         The target property. </param>
        /// <param name="source">           The target object. </param>
        /// <param name="sourceProperty">   The source property. </param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public void Bind<TSource>(string property, TSource source, string sourceProperty)
            where TSource : INotifyPropertyChanged
        {
            Bind(this, property, source, sourceProperty);
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
            Bind(target, property, SourceObject, source);
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Binds the target object's named property to the source object's named property.
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 12/24/2014. </remarks>
        ///
        /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
        ///                                         illegal values. Or, when a source property is not
        ///                                         readable.</exception>
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

            var sourceMetadata = Property.Find(sourceNotifiable.GetType(), sourceProperty);
            if (sourceMetadata == null)
                throw new ArgumentException("Cannot bind to an unregistered property.", "sourceProperty");
            if (sourceMetadata.Get == null)
                throw new ArgumentException("A source property must be readable.", "sourceProperty");

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
        /// Binds a target object property to a source object property using expressions.
        /// </summary>
        /// <typeparam name="TTarget">      The type of the target object. </typeparam>
        /// <typeparam name="TTargetValue"> The type of the target property. </typeparam>
        /// <typeparam name="TSource">      The type of the source object. </typeparam>
        /// <typeparam name="TSourceValue"> The type of the source property. </typeparam>
        /// <param name="targetObject">     The target object. </param>
        /// <param name="targetExpression"> An expression from the target object to the target
        ///                                 property. </param>
        /// <param name="sourceObject">     The source object. </param>
        /// <param name="sourceExpression"> An expression from the source object to the source
        ///                                 property. </param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public void Bind<TTarget, TTargetValue, TSource, TSourceValue>(
            TTarget targetObject,
            Expression<Func<TTarget, TTargetValue>> targetExpression,
            TSource sourceObject,
            Expression<Func<TSource, TSourceValue>> sourceExpression)
            where TTarget : INotifyPropertyChanged
            where TSource : INotifyPropertyChanged
        {
            var targetProperty = GetMemberName(targetExpression);
            var sourceProperty = GetMemberName(sourceExpression);

            Bind(targetObject, targetProperty, sourceObject, sourceProperty);
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets the name of a member specified by an expression.
        /// </summary>
        /// <param name="expression"> The expression that specifies the member. </param>
        /// <returns> The member name. </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        internal static string GetMemberName(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Lambda:
                    return GetMemberName(((LambdaExpression)expression).Body);

                case ExpressionType.MemberAccess:
                    MemberExpression me = (MemberExpression)expression;
                    var parent = GetMemberName(me.Expression);
                    return (parent == string.Empty) ? me.Member.Name : parent + "." + me.Member.Name;

                case ExpressionType.Parameter:
                    return string.Empty;

                case ExpressionType.Call:
                    var call = (MethodCallExpression)expression;
                    var name = call.Method.Name;

                    // Handle the CanWrite pseudo-property.
                    //
                    // Note: There is no CanRead pseudo-property when binding expressions because the member cannot
                    //       be read to get to the fake CanRead() method, which is needed at compile time.
                    if (name == "CanWrite")
                    {
                        return GetMemberName(call.Arguments[0]) + "." + name;
                    }

                    throw new NotSupportedException(
                        string.Format("Unsupported method call '{0}' in expression", name));

                default:
                    throw new NotSupportedException(
                        string.Format("Unsupported expression type: '{0}'", expression.NodeType));
            }
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Updates the binding target property by converting the source value to the target's type.
        /// </summary>
        ///
        /// <exception cref="InvalidConversionException"> Thrown when no <see cref="BindingConverter"/>
        ///                                               is registered between the target and source types.
        /// </exception>
        ///
        /// <param name="owner">        The target property owner. </param>
        /// <param name="targetInfo">   Information describing the target property. </param>
        /// <param name="sourceType">   The type of the source property. </param>
        /// <param name="value">        The current source property value. </param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        static void UpdateTarget(object owner, PropertyMetadata targetInfo, Type sourceType, object value)
        {
            var converterType = ConverterRegistry.Find(sourceType, targetInfo.Type);

            if (converterType == null)
                throw new InvalidConversionException(string.Format(
                    "No converter exists between {0} and {1}.",
                    sourceType,
                    targetInfo.Type));

            var converter = (BindingConverter)Activator.CreateInstance(converterType);
            var targetValue = converter.ConvertTo(value, targetInfo.Type, null);

            if (!ReferenceEquals(targetValue, BindingConverter.NoValue))
            {
                targetInfo.Set(owner, targetValue);
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
        /// <summary>   Handles <see cref="E:PropertyChanged"/> events from bound sources. </summary>
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
        /// <summary>   Handles <see cref="E:PropertyChanged"/> events from bound targets. </summary>
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

    /// <summary>
    /// Contains binding-related extension methods.
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass",
        Justification = 
        "The extension makes the most sense in the context of the class whose methods it supplements.")]
    [SuppressMessage("Microsoft.StyleCop.CSharp.OrderingRules", 
        "SA1204:StaticElementsMustAppearBeforeInstanceElements", Justification =
        "This extension is secondary to the class above and should therefore be located after it.")]
    public static class BindingObjectExtensions
    {
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Binds a target object property to a source object property using expressions.
        /// </summary>
        /// <remarks> 
        /// This is used when a binding is established outside of the target objects code.  Such as happens
        /// when a WPF Window makes a call to bind a View property to a View Model property. </remarks>
        /// <typeparam name="TTarget">      The type of the target object. </typeparam>
        /// <typeparam name="TTargetValue"> The type of the target property. </typeparam>
        /// <typeparam name="TSource">      The type of the source object. </typeparam>
        /// <typeparam name="TSourceValue"> The type of the source property. </typeparam>
        /// <param name="target">           The target object. </param>
        /// <param name="targetProperty">   An expression from the target object to the target
        ///                                 property. </param>
        /// <param name="source">           The source object. </param>
        /// <param name="sourceProperty">   An expression from the source object to the source
        ///                                 property. </param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void Bind<TTarget, TTargetValue, TSource, TSourceValue>(
            this TTarget target,
            Expression<Func<TTarget, TTargetValue>> targetProperty,
            TSource source,
            Expression<Func<TSource, TSourceValue>> sourceProperty)
            where TTarget : BindingObject
            where TSource : INotifyPropertyChanged
        {
            var targetName = BindingObject.GetMemberName(targetProperty);
            var sourceName = BindingObject.GetMemberName(sourceProperty);

            target.Bind(targetName, source, sourceName);
        }

        /// <summary>
        /// Does nothing.  This method is used to check whether or not a property can be read when using property
        /// binding expressions.  However, it is never really called because the binding mechanism replaces it with
        /// a constant value.
        /// </summary>
        /// <param name="property"> The property whose writable state is being checked. </param>
        /// <exception cref="InvalidOperationException"> Thrown if this function is called at runtime. </exception>
        /// <returns>Nothing.  Calling this function at runtime will result in an exception. </returns>
        public static bool CanWrite(this object property)
        {
            throw new InvalidOperationException("The CanWrite method should never be called directly.");
        }
    }
}
