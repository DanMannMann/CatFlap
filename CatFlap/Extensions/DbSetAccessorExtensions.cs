using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FelineSoft.CatFlap.Extensions
{
    public static class SetAccessorExtensions
    {
        //TODO rewrite these in light of changes to relationship filtering

        //private static DbContext GetContext<TEntity>(this DbSet<TEntity> dbSet)
        //where TEntity : class
        //{
        //    object internalSet = dbSet
        //        .GetType()
        //        .GetField("_internalSet", BindingFlags.NonPublic | BindingFlags.Instance)
        //        .GetValue(dbSet);
        //    object internalContext = internalSet
        //        .GetType()
        //        .BaseType
        //        .GetField("_internalContext", BindingFlags.NonPublic | BindingFlags.Instance)
        //        .GetValue(internalSet);
        //    return (DbContext)internalContext
        //        .GetType()
        //        .GetProperty("Owner", BindingFlags.Instance | BindingFlags.Public)
        //        .GetValue(internalContext, null);
        //}

        //private static Dictionary<DbContext, CatFlap> _catFlaps = new Dictionary<DbContext, CatFlap>();

        //private static CatFlap EnsureCatFlapExists<Tcontext, Tset>(DbSet<Tset> set)
        //    where Tcontext : DbContext, new()
        //    where Tset : class, new()
        //{
        //    var dbContext = set.GetContext();
        //    if (dbContext.GetType() != typeof(Tcontext))
        //    {
        //        throw new CatFlapException("Incorrect type specified in DbSet extension call - Tcontext does not match the type of the parent of the set");
        //    }
        //    if (!_catFlaps.ContainsKey(dbContext))
        //    {
        //        var func = new Func<Tcontext>(() => (Tcontext)dbContext);
        //        var instance = (CatFlap<Tcontext>)Activator.CreateInstance(typeof(CatFlap<Tcontext>), func, true);
        //        _catFlaps.Add(dbContext, instance);
        //    }
        //    return _catFlaps[dbContext];
        //}

        //public static IEnumerable<T> Query<Tcontext,Tset,T>(this DbSet<Tset> set, Func<IQueryable<Tset>, IQueryable<Tset>> linq)
        //    where Tset : class, new()
        //    where Tcontext : DbContext, new()
        //{
        //    var modeler = EnsureCatFlapExists<Tcontext, Tset>(set);
        //    return (modeler as CatFlap<Tcontext>).GetCollection<T, Tset>(linq);
        //}

        //public static IEnumerable<Tout> Query<Tcontext, Tset, Tout, Tselect>(this DbSet<Tset> set, Func<IQueryable<Tset>, IQueryable<Tselect>> linq)
        //    where Tset : class, new()
        //    where Tcontext : DbContext, new()
        //{
        //    var modeler = EnsureCatFlapExists<Tcontext, Tset>(set);
        //    return (modeler as CatFlap<Tcontext>).GetCollection<Tout, Tset, Tselect>(linq);
        //}

        //public static IEnumerable<T> QueryWhere<Tcontext, Tset, T>(this DbSet<Tset> set, Expression<Func<Tset, bool>> predicate)
        //    where Tset : class, new()
        //    where Tcontext : DbContext, new()
        //{
        //    var modeler = EnsureCatFlapExists<Tcontext, Tset>(set);
        //    return (modeler as CatFlap<Tcontext>).GetCollection<T, Tset>(predicate);
        //}

        //public static T Get<Tcontext, Tset, T>(this DbSet<Tset> set, Func<IQueryable<Tset>, IQueryable<Tset>> linq)
        //    where Tset : class, new()
        //    where Tcontext : DbContext, new()
        //{
        //    var modeler = EnsureCatFlapExists<Tcontext, Tset>(set);
        //    return (modeler as CatFlap<Tcontext>).Get<T, Tset>(linq);
        //}

        //public static Tout Get<Tcontext, Tset, Tout, Tselect>(this DbSet<Tset> set, Func<IQueryable<Tset>, IQueryable<Tselect>> linq)
        //    where Tset : class, new()
        //    where Tcontext : DbContext, new()
        //{
        //    var modeler = EnsureCatFlapExists<Tcontext, Tset>(set);
        //    return (modeler as CatFlap<Tcontext>).Get<Tout, Tset, Tselect>(linq);
        //}

        //public static T GetWhere<Tcontext, Tset, T>(this DbSet<Tset> set, Expression<Func<Tset, bool>> predicate)
        //    where Tset : class, new()
        //    where Tcontext : DbContext, new()
        //{
        //    var modeler = EnsureCatFlapExists<Tcontext, Tset>(set);
        //    return (modeler as CatFlap<Tcontext>).Get<T, Tset>(predicate);
        //}

        //public static void Update<Tcontext, Tset, T>(this DbSet<Tset> set, T input, Expression<Func<Tset, bool>> predicate = null)
        //    where Tset : class, new()
        //    where Tcontext : DbContext, new()
        //{
        //    var modeler = EnsureCatFlapExists<Tcontext, Tset>(set);
        //    (modeler as CatFlap<Tcontext>).Update<T, Tset>(input, predicate);
        //}

        //public static void Add<Tcontext, Tset, T>(this DbSet<Tset> set, T input)
        //    where Tset : class, new()
        //    where Tcontext : DbContext, new()
        //{
        //    var modeler = EnsureCatFlapExists<Tcontext, Tset>(set);
        //    (modeler as CatFlap<Tcontext>).Add<T, Tset>(input);
        //}

        //public static void AddOrUpdate<Tcontext, Tset, T>(this DbSet<Tset> set, T input)
        //    where Tset : class, new()
        //    where Tcontext : DbContext, new()
        //{
        //    var modeler = EnsureCatFlapExists<Tcontext, Tset>(set);
        //    (modeler as CatFlap<Tcontext>).AddOrUpdate<T, Tset>(input);
        //}
    }
}
