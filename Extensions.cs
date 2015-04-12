namespace RabidWarren.Binding
{
    using System;
    using System.Linq.Expressions;
    
    public static class ComposeMethods
    {
        public static Expression<Func<TGrandparent, TChild>> Compose<TGrandparent, TParent, TChild>(
            this Expression<Func<TGrandparent, TParent>> parent, Expression<Func<TParent, TChild>> child)
        {
            return CompositionVisitor<TGrandparent, TParent, TChild>.Visit(parent, child);
        }
    }
}
