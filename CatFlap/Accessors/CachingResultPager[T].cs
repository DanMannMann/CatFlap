using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelineSoft.CatFlap
{
    public class CachingResultPager<Tin, Tout, Tcontext> : CachingResultPager<Tin, Tout, Tin, Tcontext>
        where Tcontext : DbContext, new()
        where Tin : class, new()
        where Tout : class, new()
    {
        internal CachingResultPager(int resultsPerPage, Func<IQueryable<Tin>, IQueryable<Tin>> query, ISetAccessor<Tcontext, Tin> parent)
            : base(resultsPerPage, query, parent)
        {

        }
    }

    public class CachingResultPager<Tin, Tout, Tquery, Tcontext> : IResultPager<Tout>
        where Tcontext : DbContext, new()
        where Tin : class, new()
        where Tout : class, new()
        where Tquery : class, new()
    {
        private List<IEnumerable<Tout>> _resultCache;
        private int _finalPageSize = 0;
        private Func<IQueryable<Tin>, IQueryable<Tquery>> _query;
        private ISetAccessor<Tcontext, Tin> _parent;

        internal CachingResultPager(int resultsPerPage, Func<IQueryable<Tin>, IQueryable<Tquery>> query, ISetAccessor<Tcontext, Tin> parent)
        {
            _query = query;
            _parent = parent;
            ResultsPerPage = resultsPerPage;
            TotalResults = _parent.Count(query);
            PageCount = TotalResults / ResultsPerPage;
            CurrentPageIndex = -1;
            _finalPageSize = TotalResults % ResultsPerPage;
            if (_finalPageSize > 0)
            {
                PageCount++;
            }
            _resultCache = new List<IEnumerable<Tout>>(PageCount);
            for (int i = 0; i < PageCount; i++)
            {
                _resultCache.Add(null);
            }
        }

        public int ResultsPerPage { get; private set; }

        public IEnumerable<Tout> NextPage()
        {
            if (CurrentPageIndex + 1 >= PageCount)
            {
                throw new IndexOutOfRangeException("No more pages");
            }
            else
            {
                return Page(CurrentPageIndex + 1);
            }
        }

        public IEnumerable<Tout> CurrentPage()
        {
            if (CurrentPageIndex == -1)
            {
                throw new CatFlapException("You must call NextPage or access a page by index to initialise the pager before calling CurrentPage");
            }
            else
            {
                return Page(CurrentPageIndex);
            }
        }

        public IEnumerable<Tout> PreviousPage()
        {
            if (CurrentPageIndex - 1 < 0)
            {
                throw new IndexOutOfRangeException("No previous pages");
            }
            else
            {
                return Page(CurrentPageIndex - 1);
            }
        }

        private IEnumerable<Tout> Page(int index)
        {
            CurrentPageIndex = index;
            if (_resultCache[CurrentPageIndex] == null)
            {
                int toTake = ResultsPerPage;
                int toSkip = ResultsPerPage * CurrentPageIndex;
                if (CurrentPageIndex + 1 == PageCount && _finalPageSize > 0)
                {
                    toTake = _finalPageSize;
                }
                _resultCache[CurrentPageIndex] = _parent.Query<Tout,Tquery>(x => _query.Invoke(x).Skip(toSkip).Take(toTake));
            }
            return _resultCache[CurrentPageIndex];
        }

        internal IEnumerable<Tout> GetWithoutSettingCurrentIndex(int index)
        {
            if (_resultCache[index] == null)
            {
                int toTake = ResultsPerPage;
                int toSkip = ResultsPerPage * index;
                if (index + 1 == PageCount && _finalPageSize > 0)
                {
                    toTake = _finalPageSize;
                }
                _resultCache[index] = _parent.Query<Tout, Tquery>(x => _query.Invoke(x).Skip(toSkip).Take(toTake));
            }
            return _resultCache[index];
        }

        public int CurrentPageIndex { get; private set; }

        public int PageCount { get; private set; }

        public IEnumerable<Tout> this[int index]
        {
            get
            {
                if (index < 0 || index >= PageCount)
                {
                    throw new IndexOutOfRangeException();
                }
                else
                {
                    return Page(index);
                }
            }
        }

        public IEnumerator<IEnumerable<Tout>> GetEnumerator()
        {
            return new ResultPagerEnumerator<Tin, Tout, Tquery, Tcontext>(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int TotalResults
        {
            get;
            private set;
        }
    }

    public class ResultPagerEnumerator<Tin, Tout, Tquery, Tcontext> : IEnumerator<IEnumerable<Tout>>
        where Tcontext : DbContext, new()
        where Tin : class, new()
        where Tout : class, new()
        where Tquery : class, new()
    {
        private CachingResultPager<Tin, Tout, Tquery, Tcontext> _parent;
        private int index = -1;

        internal ResultPagerEnumerator(CachingResultPager<Tin, Tout, Tquery, Tcontext> parent)
        {
            _parent = parent;
        }

        public IEnumerable<Tout> Current
        {
            get
            {
                return _parent.GetWithoutSettingCurrentIndex(index);
            }
        }

        public void Dispose()
        {
            
        }

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public bool MoveNext()
        {
            if (index + 1 < _parent.PageCount)
            {
                index++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            index = 0;
        }
    }
}
