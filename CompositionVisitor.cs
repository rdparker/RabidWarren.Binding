namespace RabidWarren.Binding
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    class CompositionVisitor<TGrandparent, TParent, TChild> : ExpressionVisitor
    {
        private Expression<Func<TGrandparent, TParent>> _parent;
        private MemberInfo parentMember;
        private Expression<Func<TParent, TChild>> _child;
        private Expression childBody;

        CompositionVisitor(Expression<Func<TGrandparent, TParent>> parent, Expression<Func<TParent, TChild>> child)
        {
            _parent = parent;
            parentMember = (parent.Body as MemberExpression).Member;
            _child = child;
            childBody = child.Body;
        }

        public static Expression<Func<TGrandparent, TChild>> Visit(
            Expression<Func<TGrandparent, TParent>> parent, Expression<Func<TParent, TChild>> child)
        {
            var visitor = new CompositionVisitor<TGrandparent, TParent, TChild>(parent, child);

            return (Expression<Func<TGrandparent, TChild>>)visitor.Visit(parent);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (object.ReferenceEquals(node, _parent))
                return Expression.Lambda(Visit(node.Body), VisitAndConvert(node.Parameters, "VisitLambda"));

            return base.VisitLambda<T>(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (object.ReferenceEquals(node.Member, parentMember))
            {
                switch (childBody.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        return Expression.MakeMemberAccess(node, (childBody as MemberExpression).Member);

                    case ExpressionType.Call:
                        var c = Expression.Call((childBody as MethodCallExpression).Method, node);
                        return c;

                    default:
                        throw new NotSupportedException(
                            string.Format("Unsupported expression type: '{0}'", childBody.NodeType));
                }
            }

            return base.VisitMember(node);
        }
    }
}
