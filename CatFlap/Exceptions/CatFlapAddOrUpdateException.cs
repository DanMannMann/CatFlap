using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace FelineSoft.CatFlap
{
    public class CatFlapAddOrUpdateException : CatFlapDatabaseException
    {
        public Exception ExceptionOnUpdate { get; private set; }
        public Exception ExceptionOnAdd { get; private set; }

        public CatFlapAddOrUpdateException(string message, Exception exceptionOnUpdate, Exception exceptionOnAdd)
            : base(message)
        {
            ExceptionOnUpdate = exceptionOnUpdate;
            ExceptionOnAdd = exceptionOnAdd;
        }
    }
}
