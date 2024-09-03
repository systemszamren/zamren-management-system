using System.Linq.Expressions;
using zamren_management_system.Areas.Common.Enums;

namespace zamren_management_system.Areas.Common.LinqExtensions;

public static class LinqExtensions
{
    public static IQueryable<T>? OrderByDynamic<T>(this IQueryable<T>? query, string columnName, Order order)
    {
        if (string.IsNullOrWhiteSpace(columnName))
        {
            return query;
        }

        var parameter = Expression.Parameter(query!.ElementType, "p");
        var selector = Expression.Lambda(Expression.Property(parameter, columnName), parameter);
        var method = order == Order.Asc ? "OrderBy" : "OrderByDescending";
        var result = typeof(Queryable).GetMethods().Single(
                methodInfo => Equals(methodInfo.Name, method)
                              && methodInfo.IsGenericMethodDefinition
                              && methodInfo.GetGenericArguments().Length == 2
                              && methodInfo.GetParameters().Length == 2)
            .MakeGenericMethod(query.ElementType, selector.Body.Type)
            .Invoke(null, new object[] { query, selector });

        if (result != null) return (IQueryable<T>)result;
        return query;
    }
}