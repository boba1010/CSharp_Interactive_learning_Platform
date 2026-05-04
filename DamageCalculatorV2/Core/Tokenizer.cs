using DamageCalculatorV2.Models;

namespace DamageCalculatorV2.Core
{
    public static class Tokenizer
    {
        public static List<Token> Tokenize(string input)
        {
            input = input.Replace("\r", "").Replace("\t", "").Replace("\n", "\n ");
            List<Token> tokens = [];
            int i = 0;
            int line = 1;
            int column = 1;
            bool inString = false;
            bool inChar = false;
            while (i < input.Length)
            {
                char c = input[i];
                char nextChar = ' ';
                if (input.Length > i + 1)
                    nextChar = input[i + 1];

                // skip whitespaces
                if (char.IsWhiteSpace(c))
                {
                    column++;
                    i++;
                }

                if (c == '\n')
                {
                    line++;
                    column = 1;
                    i++;
                }

                if (char.IsLetter(c))
                {
                    string word = "";

                    while (i < input.Length && char.IsLetterOrDigit(input[i]))
                    {
                        word += input[i];
                        column++;
                        i++;
                    }

                    if (word == "int")
                        tokens.Add(new Token(TokenType.Keyword, word, line, column));
                    else if (word == "string")
                        tokens.Add(new Token(TokenType.Keyword, word, line, column));
                    else if (word == "double")
                        tokens.Add(new Token(TokenType.Keyword, word, line, column));
                    else if (word == "float")
                        tokens.Add(new Token(TokenType.Keyword, word, line, column));
                    else if (word == "char")
                        tokens.Add(new Token(TokenType.Keyword, word, line, column));
                    else if (word == "bool")
                        tokens.Add(new Token(TokenType.Keyword, word, line, column));
                    else if (word == "void")
                        tokens.Add(new Token(TokenType.Keyword, word, line, column));
                    else if (word == "true")
                        tokens.Add(new Token(TokenType.Bool, word, line, column));
                    else if (word == "false")
                        tokens.Add(new Token(TokenType.Bool, word, line, column));
                    else if (word == "if")
                        tokens.Add(new Token(TokenType.If, word, line, column));
                    else if (word == "else")
                        tokens.Add(new Token(TokenType.Else, word, line, column));
                    else if (word == "while")
                        tokens.Add(new Token(TokenType.While, word, line, column));
                    else if (word == "for")
                        tokens.Add(new Token(TokenType.For, word, line, column));
                    else if (word == "Console")
                        tokens.Add(new Token(TokenType.Console, word, line, column));
                    else if (word == "WriteLine")
                        tokens.Add(new Token(TokenType.ConsoleOutput, word, line, column));
                    else if (word == "ReadLine")
                        tokens.Add(new Token(TokenType.ConsoleInput, word, line, column));
                    else if (word == "return")
                        tokens.Add(new Token(TokenType.Return, word, line, column));
                    else if (word == "using")
                        tokens.Add(new Token(TokenType.UsingDirective, word, line, column));
                    else if (word == "switch")
                        tokens.Add(new Token(TokenType.Switch, word, line, column));
                    else if (word == "case")
                        tokens.Add(new Token(TokenType.Case, word, line, column));
                    else if (word == "default")
                        tokens.Add(new Token(TokenType.Default, word, line, column));
                    else if (word == "break")
                        tokens.Add(new Token(TokenType.Break, word, line, column));
                    else if (word == "do")
                        tokens.Add(new Token(TokenType.Do, word, line, column));
                    else
                        tokens.Add(new Token(TokenType.Identifier, word, line, column));
                }

                if (char.IsDigit(c))
                {
                    string num = "";
                    while (i < input.Length && (char.IsDigit(input[i]) || input[i] == 'D' || input[i] == 'd'
                            || input[i] == 'F' || input[i] == 'f' || input[i] == '.'))
                    {
                        num += input[i];
                        column++;
                        i++;
                    }
                    if (num.Contains('.'))
                        tokens.Add(new Token(TokenType.Double, num.Replace("D", "").Replace("d", ""), line, column));
                    else if (num.EndsWith('d') || num.EndsWith('D'))
                        tokens.Add(new Token(TokenType.Double, num.Replace("D", "").Replace("d", ""), line, column));
                    else if ((num.EndsWith('F') || num.EndsWith('f')) && num.Contains('.'))
                        tokens.Add(new Token(TokenType.Float, num.Replace("F", "").Replace("f", ""), line, column));
                    else if ((num.EndsWith('F') || num.EndsWith('f')))
                        tokens.Add(new Token(TokenType.Float, num.Replace("F", "").Replace("f", ""), line, column));
                    else
                        tokens.Add(new Token(TokenType.Int, num, line, column));
                }

                if (c == '=')
                {
                    column++;
                    tokens.Add(new Token(TokenType.Equals, "=", line, column));
                    i++;
                }
                else if (c == ';')
                {
                    column++;
                    tokens.Add(new Token(TokenType.Semicolon, ";", line, column));
                    i++;
                }
                else if (c == '+' && nextChar == '+')
                {
                    column++;
                    tokens.Add(new Token(TokenType.PlusPlus, "++", line, column));
                    i += 2;
                }
                else if (c == '+' && nextChar == '=')
                {
                    column++;
                    tokens.Add(new Token(TokenType.PlusEquals, "+=", line, column));
                    i += 2;
                }
                else if (c == '+')
                {
                    column++;
                    tokens.Add(new Token(TokenType.Plus, "+", line, column));
                    i++;
                }
                else if (c == '-' && nextChar == '-')
                {
                    column++;
                    tokens.Add(new Token(TokenType.MinusMinus, "--", line, column));
                    i += 2;
                }
                else if (c == '-' && nextChar == '=')
                {
                    column++;
                    tokens.Add(new Token(TokenType.MinusEquals, "-=", line, column));
                    i += 2;
                }
                else if (c == '-')
                {
                    column++;
                    tokens.Add(new Token(TokenType.Minus, "-", line, column));
                    i++;
                }
                else if (c == '*')
                {
                    column++;
                    tokens.Add(new Token(TokenType.Star, "*", line, column));
                    i++;
                }
                else if (c == '/')
                {
                    column++;
                    tokens.Add(new Token(TokenType.Slash, "/", line, column));
                    i++;
                }
                else if (c == '{')
                {
                    column++;
                    tokens.Add(new Token(TokenType.LeftBrace, "{", line, column));
                    i++;
                }
                else if (c == '}')
                {
                    column++;
                    tokens.Add(new Token(TokenType.RightBrace, "}", line, column));
                    i++;
                }
                else if (c == '(')
                {
                    column++;
                    tokens.Add(new Token(TokenType.LeftParenthesis, "(", line, column));
                    i++;
                }
                else if (c == ')')
                {
                    column++;
                    tokens.Add(new Token(TokenType.RightParenthesis, ")", line, column));
                    i++;
                }
                else if (c == '.')
                {
                    column++;
                    tokens.Add(new Token(TokenType.Dot, ".", line, column));
                    i++;
                }
                else if (c == '"')
                {
                    inString = !inString;
                    string str = "";
                    int quotesCount = 0;
                    while (i < input.Length)
                    {
                        if (quotesCount >= 2)
                            break;
                        str += input[i];
                        if (input[i] == '\"')
                            quotesCount++;
                        column++;
                        i++;
                    }
                    tokens.Add(new Token(TokenType.String, str, line, column));
                }
                else if (c.ToString() == "'")
                {
                    inChar = !inChar;
                    string character = "";
                    int quotesCount = 0;
                    while (i < input.Length)
                    {
                        if (quotesCount >= 2)
                            break;
                        character += input[i];
                        if (input[i].ToString() == "'")
                            quotesCount++;
                        column++;
                        i++;
                    }
                    tokens.Add(new Token(TokenType.Char, character.ToString(), line, column));
                }
                else if (c == '!' && nextChar == '=')
                {
                    column++;
                    tokens.Add(new Token(TokenType.NotEqual, "!=", line, column));
                    i += 2;
                }
                else if (c == '!')
                {
                    column++;
                    tokens.Add(new Token(TokenType.Bang, "!", line, column));
                    i++;
                }
                else if (c == '<' && nextChar == '=')
                {
                    column++;
                    tokens.Add(new Token(TokenType.LessOrEqualTo, "<=", line, column));
                    i += 2;
                }
                else if (c == '>' && nextChar == '=')
                {
                    column++;
                    tokens.Add(new Token(TokenType.GreaterOrEqualTo, ">=", line, column));
                    i += 2;
                }
                else if (c == '<')
                {
                    column++;
                    tokens.Add(new Token(TokenType.LessThan, "<", line, column));
                    i++;
                }
                else if (c == '>')
                {
                    column++;
                    tokens.Add(new Token(TokenType.GreaterThan, ">", line, column));
                    i++;
                }
                else if (c == '|' && nextChar == '|')
                {
                    column++;
                    tokens.Add(new Token(TokenType.OrOr, "||", line, column));
                    i += 2;
                }
                else if (c == '&' && nextChar == '&')
                {
                    column++;
                    tokens.Add(new Token(TokenType.AndAnd, "&&", line, column));
                    i += 2;
                }
                else if (c == ':')
                {
                    column++;
                    tokens.Add(new Token(TokenType.Colon, ":", line, column));
                    i++;
                }
            }
            return tokens;
        }
    }
}