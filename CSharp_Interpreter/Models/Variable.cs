namespace CSharp_Interpreter.Models
{
    public enum DataType
    {
        Void,
        Int,
        Double,
        Float,
        String,
        Char,
        Bool,
    }
    public class Variable
    {
        public string Name { get; set; } = null!;
        public object Value { get; set; } = null!;
        public DataType Type { get; set; }
    }
}
