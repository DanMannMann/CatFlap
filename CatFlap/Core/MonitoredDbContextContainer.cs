using System;
using System.Data.Entity;

namespace FelineSoft.CatFlap
{
    public class MonitoredDbContextContainer<TContext> : IDisposable
    where TContext : DbContext, new()
    {
        private Action _closeStats;

        public TContext Context { get; set; }

        internal MonitoredDbContextContainer(Action closeStats)
        {
            _closeStats = closeStats;
        }

        public void Dispose()
        {
            _closeStats.Invoke();
        }
    }
}