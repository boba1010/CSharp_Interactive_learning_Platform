using System;
using System.Collections.Generic;
using System.Text;

namespace DamageCalculator.Models
{
    public class Parameter
    {
        public string Name { get; set; }
        public object DefualtValue { get; set; }
        public DataType Type { get; set; }
    }
}
