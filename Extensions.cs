// -----------------------------------------------------------------------
//  <copyright file="ComposeMethods.cs" company="Ron Parker">
//   Copyright 2015 Ron Parker
//  </copyright>
//  <summary>
//   Provides an extension method for composing two Expressions.
//  </summary>
// -----------------------------------------------------------------------

namespace RabidWarren.Binding
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides an extension method for composing <see cref="Expression{TDelegate}"/> items.
    /// </summary>
    public static class ComposeMethods
    {
        /// <summary>
        /// Composes the <see cref="parent"/> expression with the child expression.
        /// </summary>
        /// <typeparam name="TGrandparent">The parent expression parameter type.</typeparam>
        /// <typeparam name="TParent">The parent expression value type and child expression parameter type.</typeparam>
        /// <typeparam name="TChild">The child expression value type.</typeparam>
        /// <param name="parent">The parent expression.</param>
        /// <param name="child">The child expression.</param>
        /// <returns></returns>
        public static Expression<Func<TGrandparent, TChild>> Compose<TGrandparent, TParent, TChild>(
            this Expression<Func<TGrandparent, TParent>> parent, Expression<Func<TParent, TChild>> child)
        {
            return CompositionVisitor<TGrandparent, TParent, TChild>.Compose(parent, child);
        }
    }
}
