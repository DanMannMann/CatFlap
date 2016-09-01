using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FelineSoft.CatFlap
{
    public interface ISetAccessor
    {
        Expression FilterExpression { get; }
    }

    public interface ISetAccessor<Tdb, Tset> : ISetAccessor
        where Tdb : DbContext, new() 
        where Tset : class, new()
    {
        Expression<Func<ICollection<Tset>, IEnumerable<Tset>>> Filter { get; }

        CatFlap<Tdb> Parent { get; }

        int Count<Tquery>(Func<IQueryable<Tset>, IQueryable<Tquery>> linq);

        IEnumerable<T> GetAll<T>();

        IEnumerable<T> Query<T>(Func<IQueryable<Tset>, IQueryable<Tset>> linq);

        IEnumerable<Tout> Query<Tout, Tselect>(Func<IQueryable<Tset>, IQueryable<Tselect>> linq);

        IEnumerable<T> Where<T>(Expression<Func<Tset, bool>> predicate);

        T Get<T>(Func<IQueryable<Tset>, IQueryable<Tset>> linq);

        Tout Get<Tout, Tselect>(Func<IQueryable<Tset>, IQueryable<Tselect>> linq);

        T GetWhere<T>(Expression<Func<Tset, bool>> predicate);

        void Update<T>(T input, Expression<Func<Tset, bool>> predicate = null);

        void Add<T>(T input);

        void AddOrUpdate<T>(T input);

        ISetAccessor<Tdb, Tset> With<Trelated>(Expression<Func<Tset, ICollection<Trelated>>> target, Expression<Func<ICollection<Trelated>, IEnumerable<Trelated>>> value);

        ISetAccessor<Tdb, Tset> Using<Tparent, Trelated>(Tparent set, Expression<Func<Tparent, ISetAccessor<Tdb, Trelated>>> target)
            where Trelated : class, new();

        ISetAccessor<Tdb, Tset> Using<Tparent, Trelated>(Expression<Func<Tparent, ISetAccessor<Tdb, Trelated>>> target)
            where Trelated : class, new();

        IResultPager<T> Page<T>(Func<IQueryable<Tset>, IQueryable<Tset>> linq, int resultsPerPage)
            where T : class, new();
    }
}
