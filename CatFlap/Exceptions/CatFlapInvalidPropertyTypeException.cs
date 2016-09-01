using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace FelineSoft.CatFlap
{
    public class CatFlapInvalidPropertyTypeException : CatFlapSetAccessorException
    {
        public CatFlapInvalidPropertyTypeException(string message)
            : base(message)
        {

        }

        public CatFlapInvalidPropertyTypeException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
