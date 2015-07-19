// -----------------------------------------------------------------------
//  <copyright file="ExtensionMethods.cs" company="Ron Parker">
//   Copyright 2015 Ron Parker
//  </copyright>
//  <summary>
//   Provides binding-related extension methods.
//  </summary>
// -----------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace RabidWarren.Binding
{
    /// <summary>
    ///     Contains binding-related extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///     Binds a target object property to a source object property using expressions.
        /// </summary>
        /// <remarks>
        ///     This is used when a binding is established outside of the target objects code.  Such as happens
        ///     when a WPF Window makes a call to bind a View property to a View Model property.
        /// </remarks>
        /// <typeparam name="TTarget">      The type of the target object. </typeparam>
        /// <typeparam name="TTargetValue"> The type of the target property. </typeparam>
        /// <typeparam name="TSource">      The type of the source object. </typeparam>
        /// <typeparam name="TSourceValue"> The type of the source property. </typeparam>
        /// <param name="target">           The target object. </param>
        /// <param name="targetProperty">   An expression from the target object to the target property.</param>
        /// <param name="source">           The source object. </param>
        /// <param name="sourceProperty">   An expression from the source object to the source property.</param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void Bind<TTarget, TTargetValue, TSource, TSourceValue>(
            this TTarget target,
            Expression<Func<TTarget, TTargetValue>> targetProperty,
            TSource source,
            Expression<Func<TSource, TSourceValue>> sourceProperty)
            where TTarget : BindingObject
            where TSource : INotifyPropertyChanged
        {
            var targetName = targetProperty.GetMemberName();
            var sourceName = sourceProperty.GetMemberName();

            target.Bind(targetName, source, sourceName);
        }

        /// <summary>
        ///     Does nothing.  This method is used to check whether or not a property can be read when using property
        ///     binding expressions.  However, it is never really called because the binding mechanism replaces it with
        ///     a constant value.
        /// </summary>
        /// <typeparam name="TProperty">    The type of the property</typeparam>
        /// <param name="property"> The property whose writable state is being checked. </param>
        /// <exception cref="InvalidOperationException"> Thrown if this function is called at runtime. </exception>
        /// <returns>Nothing.  Calling this function at runtime will result in an exception. </returns>
        [SuppressMessage("ReSharper", "UnusedParameter.Global",
            Justification = "The function is never called, but a this pointer is required.")]
        public static bool CanWrite<TProperty>(this TProperty property)
        {
            throw new InvalidOperationException("The CanWrite method should never be called directly.");
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///     Gets the name of a member specified by an expression.  This can be used for property binding
        ///     expressions or for passing around a property name without risking a mismatch in a string.
        /// </summary>
        /// <param name="expression">   The expression that specifies the member. </param>
        /// <returns>                   The member name. </returns>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public static string GetMemberName(this Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Lambda:
                    return GetMemberName(((LambdaExpression) expression).Body);

                case ExpressionType.MemberAccess:
                    var me = (MemberExpression) expression;
                    var parentExpression = me.Expression;

                    if (parentExpression.NodeType == ExpressionType.Constant ||
                        parentExpression.NodeType == ExpressionType.Parameter)
                    {
                        return me.Member.Name;
                    }

                    return parentExpression.GetMemberName() + "." + me.Member.Name;

                case ExpressionType.Call:
                    var call = (MethodCallExpression) expression;
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

        /// <summary>
        ///     Gets the value of an object's property given a property expression.
        /// </summary>
        /// <typeparam name="TObject">   The type of the object.</typeparam>
        /// <typeparam name="TProperty"> The type of its property.</typeparam>
        /// <param name="obj">           The object.</param>
        /// <param name="property">
        ///     The property expression.  These are generally of the form <code>() => Property</code>.
        /// </param>
        /// <returns>                    The property value.</returns>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "This is part of the bulic API.")]
        public static TProperty GetProperty<TObject, TProperty>(
            this TObject obj, Expression<Func<TProperty>> property)
            where TObject : INotifyingObject
            where TProperty : class
        {
            var name = property.GetMemberName();
            var values = from p in typeof (TObject).GetProperties()
                where p.Name == name
                select p.GetValue(obj, null);

            return (TProperty) values.Single();
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///     If the value of the property specified by the expression is different than the new value passed in, the
        ///     given setter will be called and the appropriate PropertyChanged notification will be sent.
        /// </summary>
        /// <typeparam name="TObject">      The type of the object containing the property.</typeparam>
        /// <typeparam name="TProperty">    The properties value type.</typeparam>
        /// <param name="obj">              The object.</param>
        /// <param name="property">
        ///     The property expression.  These are generally of the form <code>() => Property</code>.
        /// </param>
        /// <param name="newValue">         The new value for the property.</param>
        /// <param name="setter">
        ///     The action which stores the property.  Often something like <code>v => field = v</code>.
        /// </param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void SetProperty<TObject, TProperty>(
            this TObject obj, Expression<Func<TProperty>> property, TProperty newValue, Action<TProperty> setter)
            where TObject : INotifyingObject
            where TProperty : class
        {
            var p = obj.GetProperty(property);

            if (p != newValue)
            {
                setter(newValue);

                obj.RaisePropertyChanged(property);
            }
        }
    }
}