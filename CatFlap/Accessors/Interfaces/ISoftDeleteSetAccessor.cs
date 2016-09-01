using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelineSoft.CatFlap
{
    public interface ISoftDeleteSetAccessor<Tdb,Tset> : ISetAccessor<Tdb, Tset>
        where Tdb : DbContext, new() 
        where Tset : class, new()
    {
        void Delete<T>(T input);
    }
}
