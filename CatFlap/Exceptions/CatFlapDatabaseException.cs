using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace FelineSoft.CatFlap
{
    public class CatFlapDatabaseException : CatFlapException
    {
        public CatFlapDatabaseException(string message)
            : base(message)
        {

        }

        public CatFlapDatabaseException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
