using System.Linq.Expressions;

namespace Infrastructure.Extensions
{
    public static class OrderByExtensions
    {
        public static IOrderedQueryable<T> OrderByProperty<T>(this IQueryable<T> q, string SortField, bool Ascending)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "p");
            MemberExpression body = Expression.Property(parameterExpression, SortField);
            LambdaExpression lambdaExpression = Expression.Lambda(body, parameterExpression);
            string methodName = (Ascending ? "OrderBy" : "OrderByDescending");
            Type[] typeArguments =
            [
                q.ElementType,
                lambdaExpression.Body.Type
            ];
            MethodCallExpression expression = Expression.Call(typeof(Queryable), methodName, typeArguments, q.Expression, lambdaExpression);
            return (IOrderedQueryable<T>)q.Provider.CreateQuery<T>(expression);
        }
    }
}