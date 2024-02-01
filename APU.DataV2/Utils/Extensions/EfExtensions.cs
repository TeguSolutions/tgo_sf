using Microsoft.EntityFrameworkCore.Query;

namespace APU.DataV2.Utils.Extensions;


public static class EfExtensions
{
    public static IQueryable<T> If<T>(this IQueryable<T> source, bool condition, Func<IQueryable<T>, IQueryable<T>> transform)
    {
        return condition ? transform(source) : source;
    }

    public static IQueryable<T> If<T, P>(this IIncludableQueryable<T, P> source, bool condition, Func<IIncludableQueryable<T, P>, IQueryable<T>> transform) where T : class
    {
        return condition ? transform(source) : source;
    }

    public static IQueryable<T> If<T, P>(this IIncludableQueryable<T, IEnumerable<P>> source, bool condition, Func<IIncludableQueryable<T, IEnumerable<P>>, IQueryable<T>> transform) where T : class
    {
        return condition ? transform(source) : source;
    }


    public static IEnumerable<T> If<T>(this IEnumerable<T> source, bool condition, Func<IEnumerable<T>, IEnumerable<T>> transform)
    {
        return condition ? transform(source) : source;
    }

    //public static IEnumerable<T> If<T, P>(this IIncludableIEnumerable<T, P> source, bool condition, Func<IIncludableQueryable<T, P>, IQueryable<T>> transform) where T : class
    //{
    //    return condition ? transform(source) : source;
    //}

    //public static IQueryable<T> If<T, P>(this IIncludableQueryable<T, IEnumerable<P>> source, bool condition, Func<IIncludableQueryable<T, IEnumerable<P>>, IQueryable<T>> transform) where T : class
    //{
    //    return condition ? transform(source) : source;
    //}
}
