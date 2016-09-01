using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace FelineSoft.CatFlap
{
    public class CatFlapAddException : CatFlapDatabaseException
    {
        public CatFlapAddException(string message, Exception exceptionOnAdd)
            : base(message, exceptionOnAdd)
        {

        }
    }
}
