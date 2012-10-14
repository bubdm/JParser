using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JParser
{
    class InvalidJsonException : Exception
    {
        public InvalidJsonException() : base()
        {
        }

        public InvalidJsonException(string ExceptionDetails): base(ExceptionDetails)
        {
            
        }
    }
}
