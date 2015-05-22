// -----------------------------------------------------------------------
//  <copyright file="CompositionVisitor.cs" company="Ron Parker">
//   Copyright 2015 Ron Parker
//  </copyright>
//  <summary>
//   Provides an ExpressionVisitor which composes a parent expression
//   with a child expression.
//  </summary>
// -----------------------------------------------------------------------

namespace RabidWarren.Binding
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// An ExpressionVisitor which composes a parent expression with a child expression.
    /// </summary>
    /// <typeparam name="TGrandparent">The parent expression parameter type.</typeparam>
    /// <typeparam name="TParent">The parent expression value type and child expression parameter type.</typeparam>
    /// <typeparam name="TChild">The child expression value type.</typeparam>
    class CompositionVisitor<TGrandparent, TParent, TChild> : ExpressionVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositionVisitor{TGrandparent, TParent, TChild}"/> class.
        /// </summary>
        /// <param name="parent">The parent expression.</param>
        /// <param name="child">The child expression.</param>
        CompositionVisitor(Expression<Func<TGrandparent, TParent>> parent, Expression<Func<TParent, TChild>> child)
        {
            Parent = parent;
            Child = child;
        }

        /// <summary>
        /// Gets or sets the parent expression.
        /// </summary>
        Expression<Func<TGrandparent, TParent>> Parent { get; set; }

        /// <summary>
        /// Gets or sets the child expression.
        /// </summary>
        Expression<Func<TParent, TChild>> Child { get; set; }

        /// <summary>
        /// Composes two expressions using a <see cref="CompositionVisitor{TGrandparent, TParent, TChild}"/>.
        /// </summary>
        /// <param name="parent">The parent expression.</param>
        /// <param name="child">The child expression.</param>
        /// <returns>The composition of <paramref name="parent"/> and <paramref name="child"/>.</returns>
        public static Expression<Func<TGrandparent, TChild>> Compose(
            Expression<Func<TGrandparent, TParent>> parent, Expression<Func<TParent, TChild>> child)
        {
            var visitor = new CompositionVisitor<TGrandparent, TParent, TChild>(parent, child);

            return (Expression<Func<TGrandparent, TChild>>)visitor.Visit(child);
        }

        /// <summary>
        /// Visits the children of the <see cref="Expression{TDelegate}"/>, replacing the parameters of
        /// <see cref="Child"/>, with <see cref="Parent"/>.
        /// </summary>
        /// <typeparam name="T">The type of the delegate.</typeparam>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the
        /// original expression.</returns>
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (ReferenceEquals(node, Child))
                return Expression.Lambda(Visit(Child.Body), VisitAndConvert(Parent.Parameters, "VisitLambda"));

            return base.VisitLambda<T>(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="MemberExpression"/>, composing references to <see cref="Child"/>
        /// with <see cref="Parent"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the
        /// original expression.
        /// </returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            var childBody = Child.Body;

            if (ReferenceEquals(node, childBody) && childBody.NodeType == ExpressionType.MemberAccess)
                return Expression.MakeMemberAccess(Parent.Body, (childBody as MemberExpression).Member);

            return base.VisitMember(node);
        }

        /// <summary>
        /// Visits the children of the MethodCallExpression, composing any references to <c>Child</c> with
        /// <c>Parent</c>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the
        /// original expression.
        /// </returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var args = ChainParameters(Parent.Body, node.Arguments);
            
            args = VisitExpressionList(args);

            var methodInfo = node.Method;
            if (methodInfo.IsStatic)
                return Expression.Call(null, methodInfo, args);

            return Expression.Call(args[0], methodInfo, args.Skip(1));
        }

        /// <summary>
        /// Visits the children of the Expression list.
        /// </summary>
        /// <param name="original">The list of Expressions to visit.</param>
        /// <returns>
        /// The modified expression list, if any element or subexpression was modified; otherwise, returns the
        /// original expression list.
        /// </returns>
        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Expression> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                Expression p = Visit(original[i]);
                if (list != null)
                    list.Add(p);
                else if (p != original[i])
                {
                    list = new List<Expression>(n);
                    list.AddRange(original.Take(i - 1));
                    list.Add(p);
                }
            }

            if (list != null)
                return new ReadOnlyCollection<Expression>(list);

            return original;
        }

        /// <summary>
        /// Replaces any parameter usage in the expression list with <paramref name="parent"/>, effectively chaining
        /// or composing the two.
        /// </summary>
        /// <param name="parent">
        /// The parent expression to use in replacing naked parameters in the expression list.
        /// </param>
        /// <param name="arguments">
        /// A list of Expressions, which make up the arguments of a method call.
        /// </param>
        /// <returns>
        /// The modified expression list, if any element or subexpression was modified; otherwise returns the original
        /// expression list.
        /// </returns>
        private ReadOnlyCollection<Expression> ChainParameters(Expression parent, ReadOnlyCollection<Expression> arguments)
        {
            if (arguments.All(x => x.NodeType != ExpressionType.Parameter))
                return arguments;

            var newArgs = arguments.Select(x => x.NodeType == ExpressionType.Parameter ? parent : x);

            return new ReadOnlyCollection<Expression>(newArgs.ToList());
        }
    }
}
