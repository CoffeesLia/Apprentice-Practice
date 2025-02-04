using Stellantis.ProjectName.Infrastructure.Data.Extensions;
using System.Linq.Expressions;

namespace Stellantis.ProjectName.Infrastructure.Data.Extensions
{
    public static class OrderByExtensions
    {
        public static IQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> query, string? keySelector, string? orderDirection = OrderDirection.Ascending)
        {
            if (string.IsNullOrEmpty(keySelector))
                return query;
            else
            {
                var parameter = Expression.Parameter(typeof(TSource), "x");
                MemberExpression member;
                try
                {
                    member = Expression.MakeMemberAccess(parameter, typeof(TSource).GetMember(keySelector)[0]);
                }
                catch (IndexOutOfRangeException exception)
                {
                    throw new ArgumentException("Invalid propety.", nameof(keySelector), exception);
                }
                var convert = Expression.Convert(member, typeof(object));
                var keySelectorLambda = Expression.Lambda<Func<TSource, object>>(convert, parameter);

                return orderDirection switch
                {
                    null or OrderDirection.Ascending => query.OrderBy(keySelectorLambda),
                    OrderDirection.Descending => query.OrderByDescending(keySelectorLambda),
                    _ => throw new ArgumentException("Invalid order direction.", nameof(orderDirection)),
                };
            }
        }
    }
}
