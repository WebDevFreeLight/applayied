using System.Linq.Expressions;

namespace VibeVoice.Application.Common;

/// <summary>
/// Composes filter expressions so optional filters are only added when present, instead of
/// "captured == null || x.Col == captured" patterns that ORMs translate poorly.
/// </summary>
public static class Predicate
{
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var param = left.Parameters[0];
        var rightBody = new Rebind(right.Parameters[0], param).Visit(right.Body);
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left.Body, rightBody), param);
    }

    public static Expression<Func<T, bool>> AndIf<T>(this Expression<Func<T, bool>> left, bool condition, Expression<Func<T, bool>> right) =>
        condition ? left.And(right) : left;

    private sealed class Rebind(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : base.VisitParameter(node);
    }
}
