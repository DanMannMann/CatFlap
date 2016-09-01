using FelineSoft.CatFlap;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FelineSoft.CatFlap
{
    public class SoftDeleteSetAccessor<Tdb, Tset> : SetAccessor<Tdb,Tset>, ISoftDeleteSetAccessor<Tdb, Tset>
        where Tdb : DbContext, new()
        where Tset : class, new()
    {
        private Func<Tset, bool> _softDeleteSelector;

        protected internal SoftDeleteSetAccessor(CatFlap<Tdb> parent, Func<Tset, bool> softDeleteSelector, Expression<Func<ICollection<Tset>, IEnumerable<Tset>>> filterQuery)
            : base(parent, x => x.Where(softDeleteSelector))
        {
            _softDeleteSelector = softDeleteSelector;
        }

        protected internal SoftDeleteSetAccessor(CatFlap<Tdb> parent, Func<Tset, bool> softDeleteSelector)
            : base(parent, x => x.Where(softDeleteSelector))
        {
            _softDeleteSelector = softDeleteSelector;
        }

        public virtual void Delete<T>(T input)
        {

        }
    }
}
