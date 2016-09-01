using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace FelineSoft.CatFlap
{
    public class CatFlapUpdateException : CatFlapDatabaseException
    {
        public CatFlapUpdateException(string message, Exception exceptionOnUpdate)
            : base(message, exceptionOnUpdate)
        {

        }
    }
}
