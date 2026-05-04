using System;
using System.Collections.Generic;
using System.Text;

namespace DamageCalculator.Models
{
    public class Method
    {
        public string Name { get; set; }
        public DataType ReturnType { get; set; }
        public Parameter[] Parameters { get; set; }
    }
}
