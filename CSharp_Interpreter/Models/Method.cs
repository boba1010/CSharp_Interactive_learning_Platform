using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp_Interpreter.Models
{
    public class Method
    {
        public string Name { get; set; }
        public DataType ReturnType { get; set; }
        public Parameter[] Parameters { get; set; }
    }
}
