using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp_Interpreter.Exceptions
{
    public class CompilerError : Exception
    {
        public CompilerError(string msg) : base(msg) { }
    }

    public class RuntimeError(string msg) : Exception(msg)
    {
    }

    public class SecurityError(string msg) : Exception(msg) { }
}
