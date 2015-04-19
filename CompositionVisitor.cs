namespace RabidWarren.Binding
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;

    class CompositionVisitor<TGrandparent, TParent, TChild> : ExpressionVisitor
    {
        Expression<Func<TGrandparent, TParent>> Parent { get; set; }
        Expression<Func<TParent, TChild>> Child { get; set; }

        CompositionVisitor(Expression<Func<TGrandparent, TParent>> parent, Expression<Func<TParent, TChild>> child)
        {
            Parent = parent;
            Child = child;
        }

        public static Expression<Func<TGrandparent, TChild>> Visit(
            Expression<Func<TGrandparent, TParent>> parent, Expression<Func<TParent, TChild>> child)
        {
            var visitor = new CompositionVisitor<TGrandparent, TParent, TChild>(parent, child);

            return (Expression<Func<TGrandparent, TChild>>)visitor.Visit(child);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (ReferenceEquals(node, Child))
                return Expression.Lambda(Visit(Child.Body), VisitAndConvert(Parent.Parameters, "VisitLambda"));

            return base.VisitLambda<T>(node);
        }

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
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
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
    }
}
