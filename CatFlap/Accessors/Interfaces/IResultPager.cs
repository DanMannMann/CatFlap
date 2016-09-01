using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelineSoft.CatFlap
{
    public interface IResultPager<T> : IEnumerable<IEnumerable<T>>
        where T :class, new()
    {
        int ResultsPerPage { get; }
        IEnumerable<T> NextPage();
        IEnumerable<T> CurrentPage();
        IEnumerable<T> PreviousPage();
        int CurrentPageIndex { get; }
        int PageCount { get; }
        int TotalResults { get; }
        IEnumerable<T> this[int index] { get; }
    }
}
