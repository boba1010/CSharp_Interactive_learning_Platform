using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;

namespace DamageCalculatorV2.Exceptions
{
    public class TokenError : Exception
    {
        public TokenError(string msg) : base(msg) { }
    }

    public class ScopeError : Exception
    {
        public ScopeError(string msg) : base(msg) { }
    }

    public class TypeError : Exception
    {
        public TypeError(string msg) : base(msg) { }
    }

    public class SyntaxError : Exception 
    {
        public SyntaxError(string msg) : base(msg) { }
    }

    public class ExpressionError : Exception
    {
        public ExpressionError(string msg) : base(msg) { }
    }

    public class RuntimeError(string msg) : Exception(msg)
    {
    }

    public class SecurityError(string msg) : Exception(msg) { }
}
