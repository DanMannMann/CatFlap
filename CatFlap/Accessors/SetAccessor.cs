using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FelineSoft.CatFlap.Extensions;
using FelineSoft.CatFlap.Utils;

namespace FelineSoft.CatFlap
{
    public class SetAccessor<Tdb, Tset> : ISetAccessor<Tdb, Tset>
        where Tdb : DbContext, new() 
        where Tset : class, new()
    {
        private CatFlap<Tdb> _parent;
        private Expression<Func<ICollection<Tset>, IEnumerable<Tset>>> _filter;
                         //Target   //Query to apply
        internal Dictionary<Expression, Expression> _relatedEntityFilters;
        internal Lictionary<Type, PropertyInfo> _usingRelationships;

        private SetAccessor(CatFlap<Tdb> parent, Expression<Func<ICollection<Tset>, IEnumerable<Tset>>> filterQuery, Dictionary<Expression, Expression> relatedEntityFilters, Lictionary<Type, PropertyInfo> usingRelationships)
        {
            _relatedEntityFilters = relatedEntityFilters;
            _parent = parent;
            _filter = filterQuery;
            _usingRelationships = usingRelationships;
        }

        protected internal SetAccessor(CatFlap<Tdb> parent)
        {
            _parent = parent;
            _filter = x => x;
            _relatedEntityFilters = new Dictionary<Expression, Expression>();
            _usingRelationships = new Lictionary<Type, PropertyInfo>();
        }

        protected internal SetAccessor(CatFlap<Tdb> parent, Expression<Func<ICollection<Tset>, IEnumerable<Tset>>> filterQuery)
        {
            _parent = parent;
            _filter = filterQuery;
            _relatedEntityFilters = new Dictionary<Expression, Expression>();
            _usingRelationships = new Lictionary<Type, PropertyInfo>();
        }

        public Expression<Func<ICollection<Tset>, IEnumerable<Tset>>> Filter
        {
            get { return _filter; }
        }

        public Expression FilterExpression
        {
            get
            {
                return _filter as Expression;
            }
        }

        public CatFlap<Tdb> Parent
        {
            get { return _parent; }
        }

        public virtual IEnumerable<T> Query<T>(Func<IQueryable<Tset>, IQueryable<Tset>> linq)
        {
            return _parent.GetCollection<T, Tset>(x => linq.Invoke(x), _relatedEntityFilters, _usingRelationships);
        }

        public virtual IEnumerable<T> GetAll<T>()
        {
            return _parent.GetCollection<T, Tset>(x => x, _relatedEntityFilters, _usingRelationships);
        }

        public virtual int Count<Tquery>(Func<IQueryable<Tset>, IQueryable<Tquery>> linq)
        {
            return _parent.Count<Tset, Tquery>(x => linq.Invoke(x));
        }

        public virtual ISetAccessor<Tdb, Tset> With<Trelated>(Expression<Func<Tset, ICollection<Trelated>>> target, Expression<Func<ICollection<Trelated>, IEnumerable<Trelated>>> value)
        {
            var newFilters = _relatedEntityFilters.Clone();
            newFilters.Add(target, value);
            return new SetAccessor<Tdb, Tset>(_parent, _filter, newFilters, _usingRelationships.Clone());
        }

        public virtual ISetAccessor<Tdb, Tset> Using<Tparent, Trelated>(Tparent set, Expression<Func<Tparent, ISetAccessor<Tdb, Trelated>>> target)
            where Trelated : class, new()
        {
            var newUsings = _usingRelationships.Clone();
            newUsings.Add(typeof(Trelated), (target.Body as MemberExpression).Member as PropertyInfo);
            return new SetAccessor<Tdb, Tset>(_parent, _filter, _relatedEntityFilters.Clone(), newUsings);
        }

        public virtual ISetAccessor<Tdb, Tset> Using<Tparent, Trelated>(Expression<Func<Tparent, ISetAccessor<Tdb, Trelated>>> target)
            where Trelated : class, new()
        {
            var newUsings = _usingRelationships.Clone();
            newUsings.Add(typeof(Trelated), (target.Body as MemberExpression).Member as PropertyInfo);
            return new SetAccessor<Tdb, Tset>(_parent, _filter, _relatedEntityFilters.Clone(), newUsings);
        }

        public virtual IEnumerable<Tout> Query<Tout, Tselect>(Func<IQueryable<Tset>, IQueryable<Tselect>> linq)
        {
            return _parent.GetCollection<Tout, Tset, Tselect>(x => linq.Invoke(x), _relatedEntityFilters, _usingRelationships);
        }

        public virtual IEnumerable<T> Where<T>(Expression<Func<Tset, bool>> predicate)
        {
            return _parent.GetCollection<T, Tset>(x => x.Where(predicate), _relatedEntityFilters, _usingRelationships);
        }

        public virtual T Get<T>(Func<IQueryable<Tset>, IQueryable<Tset>> linq)
        {
            return _parent.Get<T, Tset>(x => linq.Invoke(x), _relatedEntityFilters, _usingRelationships);
        }

        public virtual Tout Get<Tout, Tselect>(Func<IQueryable<Tset>, IQueryable<Tselect>> linq)
        {
            return _parent.Get<Tout, Tset, Tselect>(x => linq.Invoke(x), _relatedEntityFilters, _usingRelationships);
        }

        public virtual T GetWhere<T>(Expression<Func<Tset, bool>> predicate)
        {
            return _parent.Get<T, Tset>(x => x.Where(predicate), _relatedEntityFilters, _usingRelationships);
        }

        public virtual void Update<T>(T input, Expression<Func<Tset, bool>> predicate = null)
        {
            _parent.Update<T, Tset>(input, predicate);
        }

        public virtual void Add<T>(T input)
        {
            _parent.Add<T, Tset>(input);
        }

        public virtual void AddOrUpdate<T>(T input)
        {
            _parent.AddOrUpdate<T, Tset>(input);
        }

        public virtual IResultPager<T> Page<T>(Func<IQueryable<Tset>, IQueryable<Tset>> linq, int resultsPerPage)
            where T : class, new()
        {
            return new CachingResultPager<Tset, T, Tdb>(resultsPerPage, linq, this);
        }
    }
}

