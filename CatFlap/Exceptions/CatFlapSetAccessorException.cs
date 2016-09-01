using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace FelineSoft.CatFlap
{
    public class CatFlapSetAccessorException : CatFlapException
    {
        public CatFlapSetAccessorException(string message)
            : base(message)
        {

        }

        public CatFlapSetAccessorException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
