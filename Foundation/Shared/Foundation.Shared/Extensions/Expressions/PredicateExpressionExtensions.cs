using System.Linq.Expressions;

namespace TodoWebApp.Foundation.Shared.Extensions.Expressions;

public static class PredicateExpressionExtensions
{
  public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expression,
    Expression<Func<T, bool>> extensionExpression) =>
    Expression.Lambda<Func<T, bool>>(Expression.OrElse(expression.Body,
      Expression.Invoke(extensionExpression, expression.Parameters)), expression.Parameters);

  public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expression,
    Expression<Func<T, bool>> extensionExpression) =>
    Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expression.Body,
      Expression.Invoke(extensionExpression, expression.Parameters)), expression.Parameters);

  public static Expression<Func<T2, bool>> RetypeAs<T1, T2>(this Expression<Func<T1, bool>> expression)
  {
    var inputParameter = Expression.Parameter(typeof(T2));
    var outputParameter = Expression.TypeAs(inputParameter, typeof(T1));
    return Expression.Lambda<Func<T2, bool>>(Expression.Invoke(expression, outputParameter), inputParameter);
  }
}