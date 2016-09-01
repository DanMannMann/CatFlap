using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace FelineSoft.CatFlap
{
    public class CatFlapException : Exception
    {
        public CatFlapException(string message)
            : base(message)
        {

        }

        public CatFlapException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
