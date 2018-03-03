using System;
using System.Collections.Generic;
using System.Text;

namespace Sorry
{
    public class IncompleteTurnException : Exception
        {
            public IncompleteTurnException()
            {
            }

            public IncompleteTurnException(string message)
                : base(message)
            {
            }

            public IncompleteTurnException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }
}
