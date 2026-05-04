namespace DamageCalculatorV2.Models
{
    public enum TokenType
    {
        Keyword,
        Identifier,
        Int,
        Double,
        Float,
        Method,
        Bool,
        String,
        Char,
        Equals,
        Semicolon,
        Plus,
        Minus,
        Star,
        Slash,
        AndAnd,
        OrOr,
        Bang,
        NotEqual,
        LessThan,
        GreaterThan,
        LessOrEqualTo,
        GreaterOrEqualTo,
        RightBrace,
        LeftBrace,
        LeftParenthesis,
        RightParenthesis,
        If,
        Else,
        Console,
        ConsoleOutput,
        ConsoleInput,
        While,
        PlusPlus,
        MinusMinus,
        PlusEquals,
        MinusEquals,
        StarEquals,
        SlashEquals,
        For,
        Dot,
        Void,
        Return,
        UsingDirective,
        Switch,
        Case,
        Default,
        Break,
        Colon,
        Do,
        Unknown,
        EOF,
    }

    public class Token
    {
        public TokenType Type;
        public string Value;
        public int LineNumber;
        public int ColumnNumber;
        private TokenType leftBrace;

        public Token(TokenType leftBrace, string value)
        {
            this.leftBrace = leftBrace;
            Value = value;
        }

        public Token(TokenType type, string value, int lineNumber)
        {
            Type = type;
            Value = value;
            LineNumber = lineNumber;
        }

        public Token(TokenType type, string value, int lineNumber, int columnNumber)
        {
            Type = type;
            Value = value;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }
    }
}
