using CSharp_Interpreter.Core;
using CSharp_Interpreter.Exceptions;
using CSharp_Interpreter.Models;
using CSharp_Interpreter.Utils;
using System.Diagnostics;

public class Program
{
    private static Stack<Dictionary<string, Variable>> variables = [];
    private static Stack<Dictionary<string, Method>> methods = [];
    private static List<Token> tokens = [];
    private static int current = 0;

    public static void Main()
    {
        File.WriteAllText("temp.py", "");
        var path = Utils.Input("Enter the file path: ");
        if (string.IsNullOrEmpty(path))
            return;
        string codeText = File.ReadAllText(path);
        tokens = Tokenizer.Tokenize(codeText);

        GatherMethodDeclarations();

        List<StatementNode> statements = [];
        for (int i = 0; i < tokens.Count; i++)
            statements.Add(ParseStatement());

        BlockNode block = null;
        foreach (var stmt in statements)
        {
            if (stmt is BlockNode scope)
                block = scope;
        }

        string pyCode = CodeGenerator.GeneratePyCode(block);
        File.AppendAllText("temp.py", pyCode);

        // runs the python interpreter to execute code
        var process = new Process();
        process.StartInfo.FileName = "python";
        process.StartInfo.Arguments = "temp.py";
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        Console.WriteLine(output);
    }

    private static Token Peek(int index = 0)
    {
        if (current >= tokens.Count)
            return new(TokenType.EOF, "END", tokens[current - 1].LineNumber);
        return tokens[current + index];
    }

    private static Token Advance()
    {
        if (current != tokens.Count)
            return tokens[current++];
        return tokens[^1];
    }

    private static Token Consume(TokenType type, string errMsg)
    {
        while (Peek().Type == TokenType.Unknown)
            Advance();
        return Peek().Type == type ? Advance() : throw new CompilerError($"{errMsg}");
    }
    private static Token Consume(TokenType type)
    {
        while (Peek().Type == TokenType.Unknown)
            Advance();
        return Peek().Type == type ? Advance() : null;
    }

    private static bool Match(TokenType type, int index = 0)
    {
        return Peek(index).Type == type;
    }

    public static void PushVariableScope()
    {
        variables.Push([]);
    }

    public static void PopVariableScope()
    {
        variables.Pop();
    }

    private static StatementNode ParseBlock()
    {
        List<StatementNode> statements = [];
        Consume(TokenType.LeftBrace, "Expected '{'");
        PushVariableScope();
        while (!Match(TokenType.RightBrace))
        {
            var statement = ParseStatement();
            if (statement != null)
                statements.Add(statement);
        }

        Consume(TokenType.RightBrace, "Expected '}'");
        PopVariableScope();
        return new BlockNode { Statements = statements };
    }

    private static StatementNode ParseForBlock()
    {
        List<StatementNode> statements = [];
        Consume(TokenType.LeftBrace, "Expected '{'");
        while (!Match(TokenType.RightBrace))
        {
            var statement = ParseStatement();
            if (statement != null)
                statements.Add(statement);
        }

        Consume(TokenType.RightBrace, "Expected '}'");
        return new BlockNode { Statements = statements };
    }

    private static StatementNode ParseStatement()
    {
        Token currentToken = Peek();
        currentToken = Peek();
        if (currentToken.Type == TokenType.EOF)
            return null;
        try
        {
            switch (currentToken.Type)
            {
                case TokenType.Keyword:
                    if (Peek(1).Type == TokenType.Identifier && Peek(2).Type == TokenType.LeftParenthesis)
                        return ParseMethodDeclaration();
                    return ParseDeclaration();
                case TokenType.Identifier:
                    if (Peek(1).Type == TokenType.LeftParenthesis)
                        return ParseMethodCall();
                    return ParseAssignment();
                case TokenType.If:
                    return ParseIf();
                case TokenType.While:
                    return ParseWhile();
                case TokenType.For:
                    return ParseFor();
                case TokenType.Console:
                    return ParseConsoleMethodType();
                case TokenType.LeftBrace:
                    return ParseBlock();
                case TokenType.Return:
                    return ParseReturn();
                case TokenType.Unknown:
                    Advance();
                    return null;
                default:
                    throw new CompilerError($"Unexpected token '{currentToken.Value}' at line: {currentToken.LineNumber}, column: {currentToken.ColumnNumber}.");
            }
        }
        catch (CompilerError ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
            return null;
        }
    }

    private static void AddVariable(Variable @var)
    {
        try
        {
            var currentScope = variables.Peek();

            if (currentScope.ContainsKey(var.Name))
                throw new CompilerError($"A local variable named '{var.Name}' is already defined in this scope at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");

            currentScope.Add(var.Name, new Variable 
            {
                Name = var.Name,
                Type = var.Type,
                Value = var.Value,
            });        
        }
        catch (CompilerError ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(0);
        }
    }

    private static StatementNode ParseDeclaration()
    {
        Token typeToken = Consume(TokenType.Keyword, $"Expected variable type at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
        DataType type = ParseDataType(typeToken.Value); // convert string to enum
        if (type == DataType.Void)
            throw new CompilerError("A variable cannot be of type void.");
        Token nameToken = Consume(TokenType.Identifier, $"Expected variable name at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
        string name = nameToken.Value;

        Consume(TokenType.Equals, $"Expected '=' after variable name at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");

        ExpressionNode expr = ParseExpression();
        if (expr is LiteralNode ln)
        {
            if (type != ln.Type) 
                throw new CompilerError($"Cannot implicitly convert type '{type.ToString().ToLower()}' to '{ln.Type.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
        }

        Consume(TokenType.Semicolon, $"Expected ';' at end of statement at line: {Peek().LineNumber - 1}.");

        AddVariable(new()
        {
            Name = name,
            Type = type,
            Value = expr,
        });

        return new DeclarationNode { Name = name, Type = type, Expression = expr };
    }

    private static AssignementNode GetSelfAssignmentType(Variable variable)
    {
        try
        {
            // +=, ++, --, -= are treated the same way because python doesn't support ++, --
            AssignmentType assignmentType = AssignmentType.Equal;
            if (Match(TokenType.PlusEquals))
            {
                Advance();
                assignmentType = AssignmentType.PlusEqual;
            }
            else if (Match(TokenType.PlusPlus))
            {
                if (variable.Type == DataType.String || variable.Type == DataType.Char)
                    throw new CompilerError("Cannot use '++' on 'string' or 'char'");
                Advance();
                assignmentType = AssignmentType.PlusPlus;
                return new AssignementNode
                {
                    Expression = new LiteralNode { Value = 1, Type = DataType.Int },
                    Name = variable.Name,
                    AssignmentType = assignmentType,
                    Type = DataType.Int
                };
            }
            else if (Match(TokenType.MinusEquals))
            {
                if (variable.Type == DataType.String || variable.Type == DataType.Char)
                    throw new CompilerError("Cannot use '-' on 'string' or 'char'");
                Advance();
                assignmentType = AssignmentType.MinusEqual;
            }
            else if (Match(TokenType.MinusMinus))
            {
                if (variable.Type == DataType.String || variable.Type == DataType.Char)
                    throw new CompilerError("Cannot use '-' on 'string' or 'char'");
                Advance();
                assignmentType = AssignmentType.MinusMinus;
                return new AssignementNode
                {
                    Expression = new LiteralNode { Value = 1, Type = DataType.Int },
                    Name = variable.Name,
                    AssignmentType = assignmentType,
                    Type = DataType.Int
                };
            }
            else if (Match(TokenType.StarEquals))
            {
                if (variable.Type == DataType.String || variable.Type == DataType.Char)
                    throw new CompilerError("Cannot use '*' on 'string' or 'char'");
                Advance();
                assignmentType = AssignmentType.StarEqual;
            }
            else if (Match(TokenType.SlashEquals))
            {
                if (variable.Type == DataType.String || variable.Type == DataType.Char)
                    throw new CompilerError("Cannot use '/' on 'string' or 'char'");
                Advance();
                assignmentType = AssignmentType.SlashEqual;
            }
            return new AssignementNode
            {
                AssignmentType = AssignmentType.Equal,
            };
        }
        catch (CompilerError ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(0);
            return null;
        }
    }

    private static StatementNode ParseAssignment()
    {
        Token nameToken = null;
        try
        {
            nameToken = Consume(TokenType.Identifier, $"Invalid expression at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}..");
            var currentScope = variables.Peek();
            Variable variable = null;
            variable = currentScope[nameToken.Value];
            var assignmentType = GetSelfAssignmentType(variable);

            ExpressionNode expr = null;
            if (assignmentType.AssignmentType == AssignmentType.Equal)
                Consume(TokenType.Equals, $"Expected '=' after variable name at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}..");
        
            if (assignmentType.AssignmentType != AssignmentType.PlusPlus &&
                    assignmentType.AssignmentType != AssignmentType.MinusMinus)
                expr = ParseExpression();

            DataType variableType = variable.Type;

            Consume(TokenType.Semicolon, $"Expected ';' at end of statement at line: {Peek().LineNumber}.");

            if (expr is LiteralNode ln)
            {
                if (variableType != ln.Type)
                    throw new CompilerError($"Cannot implicitly convert type '{ln.Type.ToString().ToLower()}' to '{variableType.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            }
            else if (expr is VariableNode vn)
            {
                var variable1 = currentScope[vn.Name];
                if (variable1 == null)
                    throw new CompilerError($"The name '{vn.Name}' doesn't exist in this scope at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}..");
                if (variable1.Type != variableType)
                    throw new CompilerError($"Cannot implicitly convert type '{variable1.Type.ToString().ToLower()}' to '{variableType.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            }

            currentScope.Remove(nameToken.Value);
            currentScope.Add(nameToken.Value, new()
            {
                Name = variable.Name,
                Type = variableType,
                Value = expr
            });

            return new AssignementNode
            {
                Expression = expr,
                Name = variable.Name,
                Type = variableType,
                AssignmentType = assignmentType.AssignmentType
            };
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine($"Error: The name '{nameToken.Value}' doesn't exist in this scope at line: {nameToken.LineNumber}, column: {nameToken.ColumnNumber}.");
            Environment.Exit(1);
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
            return null;
        }
    }

    private static BinaryExpressionNode MakeBinaryExpression(ExpressionNode left, TokenType op, ExpressionNode right)
    {
        try
        {
            if (op == TokenType.Plus)
            {
                if (left.Type == DataType.Int &&
                    right.Type == DataType.Int)
                {
                    return new BinaryExpressionNode { Type = DataType.Int, Left = left, Operator = "+", Right = right };
                }
                if (left.Type == DataType.Double &&
                    right.Type == DataType.Double)
                {
                    return new BinaryExpressionNode { Type = DataType.Double, Left = left, Operator = "+", Right = right };
                }
                if (left.Type == DataType.Float &&
                    right.Type == DataType.Float)
                {
                    return new BinaryExpressionNode { Type = DataType.Float, Left = left, Operator = "+", Right = right };
                }
                if (left.Type == DataType.String &&
                    right.Type == DataType.String)
                {
                    return new BinaryExpressionNode { Type = DataType.String, Left = left, Operator = "+", Right = right };
                }
                if (left.Type == DataType.Char &&
                    right.Type == DataType.Char)
                {
                    return new BinaryExpressionNode { Type = DataType.Char, Left = left, Operator = "+", Right = right };
                }
                throw new CompilerError($"Invalid types for '+' operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            }
            if (op == TokenType.Minus)
            {
                if (left.Type == DataType.Int &&
                    right.Type == DataType.Int)
                {
                    return new BinaryExpressionNode { Type = DataType.Int, Left = left, Operator = "-", Right = right };
                }
                if (left.Type == DataType.Double &&
                    right.Type == DataType.Double)
                {
                    return new BinaryExpressionNode { Type = DataType.Double, Left = left, Operator = "-", Right = right };
                }
                if (left.Type == DataType.Float &&
                    right.Type == DataType.Float)
                {
                    return new BinaryExpressionNode { Type = DataType.Float, Left = left, Operator = "-", Right = right };
                }
                throw new CompilerError($"Invalid types for '-' operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            }
            if (op == TokenType.Star)
            {
                if (left.Type == DataType.Int &&
                    right.Type == DataType.Int)
                {
                    return new BinaryExpressionNode { Type = DataType.Int, Left = left, Operator = "*", Right = right };
                }
                if (left.Type == DataType.Double &&
                    right.Type == DataType.Double)
                {
                    return new BinaryExpressionNode { Type = DataType.Double, Left = left, Operator = "*", Right = right };
                }
                if (left.Type == DataType.Float &&
                    right.Type == DataType.Float)
                {
                    return new BinaryExpressionNode { Type = DataType.Float, Left = left, Operator = "*", Right = right };
                }
                if (left.Type == DataType.String &&
                    right.Type == DataType.Int)
                {
                    return new BinaryExpressionNode { Type = DataType.String, Left = left, Operator = "*", Right = right };
                }
                throw new CompilerError($"Invalid types for '*' operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            }
            if (op == TokenType.Slash)
            {
                if (left.Type == DataType.Int &&
                    right.Type == DataType.Int)
                {
                    return new BinaryExpressionNode { Type = DataType.Int, Left = left, Operator = "/", Right = right };
                }
                if (left.Type == DataType.Double &&
                    right.Type == DataType.Double)
                {
                    return new BinaryExpressionNode { Type = DataType.Double, Left = left, Operator = "/", Right = right };
                }
                if (left.Type == DataType.Float &&
                    right.Type == DataType.Float)
                {
                    return new BinaryExpressionNode { Type = DataType.Float, Left = left, Operator = "/", Right = right };
                }
                throw new CompilerError($"Invalid types for '/' operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            }
            throw new CompilerError($"Invalid operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
        }
        catch (CompilerError ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(0);
            return null;
        }
    }

    // first
    private static ExpressionNode ParsePrimary()
    {
        try
        {
            if (Match(TokenType.Bool))
            {
                string value = Advance().Value.ToString();
                value = char.ToUpper(value[0]) + value[1..];
                return new LiteralNode { Value = value, Type = DataType.Bool };
            }
            if (Match(TokenType.Int)) return new LiteralNode { Value = Advance().Value, Type = DataType.Int };
            if (Match(TokenType.Double)) return new LiteralNode { Value = Advance().Value, Type = DataType.Double };
            if (Match(TokenType.Float)) return new LiteralNode { Value = Advance().Value, Type = DataType.Float };
            if (Match(TokenType.String)) return new LiteralNode { Type = DataType.String, Value = Advance().Value };
            if (Match(TokenType.Identifier)) return new VariableNode { Type = ParseDataType(Peek().Type.ToString().ToLower(), Peek().Value), Name = Advance().Value };
            if (Match(TokenType.LeftParenthesis))
            {
                Consume(TokenType.LeftParenthesis, "Expected a '('");
                var expr = ParseExpression();
                Consume(TokenType.RightParenthesis, "Expected a ')'");
                return expr;
            }

            throw new CompilerError($"Expected expression at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
        }
        catch (CompilerError ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(0);
            return null;
        }
    }

    // above primary
    // second
    // handles negative and '!'
    private static ExpressionNode ParseUnary()
    {
        if (Match(TokenType.Bang) || 
            Match(TokenType.Minus))
        {
            var op = Peek().Value;
            Advance();
            var right = ParseUnary();
            return new UnaryExpressionNode { Operand = right, Operator = op };
        }
        return ParsePrimary();
    }

    // above term
    // third
    // handles *, /
    private static ExpressionNode ParseFactor()
    {
        var left = ParseUnary();
        while (Match(TokenType.Minus) ||
            Match(TokenType.Plus))
        {
            var op = Advance().Type;
            var right = ParseUnary();
            left = MakeBinaryExpression(left, op, right);
        }
        //Advance();
        return left;
    }

    // above factor
    // fourth
    // handled +, -
    private static ExpressionNode ParseTerm()
    {
        var left = ParseFactor();
        while (Match(TokenType.Minus) || 
            Match(TokenType.Plus))
        {
            var op = Peek().Type;
            var right = ParseFactor();
            left = MakeBinaryExpression(left, op, right);
        }
        //Advance();
        return left;
    }

    // above term
    // fifth
    // handles >, >=, <, <=
    private static ExpressionNode ParseComparision()
    {
        var expr = ParseTerm();
        while (Match(TokenType.GreaterThan) || 
            Match(TokenType.GreaterOrEqualTo) ||
            Match(TokenType.LessThan) ||
            Match(TokenType.LessOrEqualTo))
        {
            var op = Advance();
            var right = ParseTerm();
            expr = new BinaryExpressionNode { Left = expr, Operator = op.Value, Right = right, Type = expr.Type };
        }
        //Advance();
        return expr;
    }

    // above comparision
    // sixth
    // handles ==, !=
    private static ExpressionNode ParseEquality()
    {
        var expr = ParseComparision();
        while (Match(TokenType.Equals) ||
            Match(TokenType.NotEqual))
        {
            var op = Advance();
            var right = ParseComparision();
            expr = new BinaryExpressionNode { Left = expr, Operator = op.Value, Right = right, Type = expr.Type };
        }
        //Advance();
        return expr;
    }

    // above equality
    // seventh
    // handles &&
    private static ExpressionNode ParseAnd()
    {
        var expr = ParseEquality();
        while (Match(TokenType.AndAnd))
        {
            var op = Advance();
            var right = ParseEquality();
            expr = new BinaryExpressionNode { Left = expr, Operator = MapOperator(op.Value), Right = right, Type = expr.Type };
        }
        //Advance();
        return expr;
    }

    // above and
    // eighth
    // handles ||
    private static ExpressionNode ParseOr()
    {
        var expr = ParseAnd();
        while (Match(TokenType.OrOr))
        {
            var op = Advance();
            var right = ParseAnd();
            expr = new BinaryExpressionNode { Left = expr, Operator = MapOperator(op.Value), Right = right, Type = expr.Type };
        }
        //Advance();
        return expr;
    }

    public static string MapOperator(string op)
    {
        return op switch
        {
            "||" => "or",
            "&&" => "and",
            "!" => "not",
            _ => throw new CompilerError($"Invalid operator")
        };
    }

    private static ExpressionNode ParseExpression()
    {
        var or = ParseOr();
        return or;
    }

    private static DataType ParseDataType(string typeStr, string identifier = "")
    {
        if (typeStr == "identifier")
        {
            var currentScope = variables.Peek();
            Variable v = null;
            try
            {
                v = currentScope[identifier];
            }
            catch (KeyNotFoundException)
            {
                throw new CompilerError($"The name '{identifier}' doesn't exist in this scope at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}..");
            }
            return ParseDataType(v.Type.ToString().ToLower());
        }
        return typeStr switch
        {
            "void" => DataType.Void,
            "int" => DataType.Int,
            "double" => DataType.Double,
            "float" => DataType.Float,
            "bool" => DataType.Bool,
            "char" => DataType.Char,
            "string" => DataType.String,
            _ => throw new Exception($"Unknown type: '{typeStr}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.")
        };
    }

    private static StatementNode ParseIf()
    {
        Consume(TokenType.If);
        Consume(TokenType.LeftParenthesis, $"Expected '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
        var expr = ParseExpression();
        expr.Type = DataType.Bool;
        Consume(TokenType.RightParenthesis, $"Expected ')' at line: {Peek().LineNumber - 1}");
        var thenStatements = ParseStatement();
        StatementNode elseStatements = null;
        List<ElseIfNode> elifs = [];
        while (Match(TokenType.Else) && Match(TokenType.If, 1))
        {
            current += 2;
            Consume(TokenType.LeftParenthesis, $"Expected '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            var elifExpr = ParseExpression();
            elifExpr.Type = DataType.Bool;
            Consume(TokenType.RightParenthesis, $"Expected ')' at line: {Peek().LineNumber - 1}.");
            var elifThenStatements = ParseStatement();
            var elif = new ElseIfNode
            {
                Condition = elifExpr,
                ThenStatements = elifThenStatements
            };
            elifs.Add(elif);
        }
        if (Match(TokenType.Else))
        {
            Advance();
            elseStatements = ParseStatement();
        }
        return new IfNode
        {
            Condition = expr,
            ThenStatements = thenStatements,
            ElseStatements = elseStatements,
            ElseIfStatements = elifs
        };
    }

    private static StatementNode ParseWhile()
    {
        Consume(TokenType.While);
        Consume(TokenType.LeftParenthesis, $"Expected '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
        var expr = ParseExpression();
        expr.Type = DataType.Bool;
        Consume(TokenType.RightParenthesis, $"Expected ')' at line: {Peek().LineNumber - 1}.");
        StatementNode body = ParseStatement();
        return new WhileNode
        {
            Body = body,
            Condition = expr
        };
    }

    private static StatementNode ParseFor()
    {
        Consume(TokenType.For);
        Consume(TokenType.LeftParenthesis, $"Expected '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
        PushVariableScope();

        // for (*int i = 0;* i < length;)
        DeclarationNode startPoint = (DeclarationNode)ParseDeclaration();

        // for (int i = 0; *i < length;*)
        var expr = ParseExpression();
        expr.Type = DataType.Bool;
        Consume(TokenType.Semicolon, $"Expected ';' at line: {Peek().LineNumber - 1}.");

        // for (int i = 0; i < length; *i++*)
        AssignementNode steps = null;
        if (Peek().Type == TokenType.Identifier)
        {
            Advance();
            steps = GetSelfAssignmentType(new Variable
            {
                Name = startPoint.Name,
                Value = startPoint.Expression,
                Type = startPoint.Type
            });
        }

        Consume(TokenType.RightParenthesis, $"Expected ')' at line: {Peek().LineNumber - 1}.");
        ForExpression forExpr = new()
        {
            Condition = expr,
            StartPoint = startPoint,
            Steps = steps
        };

        StatementNode body = ParseForBlock();
        PopVariableScope();
        return new ForNode
        {
            ForExpression = forExpr,
            Body = body
        };
    }

    private static ConsoleNode ParseConsoleMethodType()
    {
        Consume(TokenType.Console);
        Consume(TokenType.Dot);
        if (Match(TokenType.ConsoleOutput))
            return ParseConsoleOutputMethod();
        if (Match(TokenType.ConsoleInput))
            return ParseConsoleInputMethod();
        throw new Exception("Invalid Console method.");
    }

    private static ConsoleNode ParseConsoleOutputMethod()
    {
        Consume(TokenType.ConsoleOutput);
        Consume(TokenType.LeftParenthesis, "Expected '('");
        if (Peek().Type == TokenType.RightParenthesis)
        {
            Advance();
            Consume(TokenType.Semicolon, "Expected ';'");
            return new ConsoleOutputNode { Object = new LiteralNode { Value = '\n' } };
        }
        var expr = ParseExpression();
        Consume(TokenType.RightParenthesis, "Expected ')'");
        Consume(TokenType.Semicolon, "Expected ';'");
        return new ConsoleOutputNode { Object = expr };
    }

    private static ConsoleNode ParseConsoleInputMethod()
    {
        Consume(TokenType.ConsoleInput);
        Consume(TokenType.LeftParenthesis, $"Expected '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
        if (Peek().Type == TokenType.RightParenthesis)
        {
            Advance();
            Consume(TokenType.Semicolon, $"Expected ';' at line: {Peek().LineNumber - 1}.");
            return new ConsoleInputNode { };
        }
        Consume(TokenType.RightParenthesis, $"Expected ')' at line: {Peek().LineNumber - 1}.");
        Consume(TokenType.Semicolon, $"Expected ';' at line: {Peek().LineNumber - 1}.");
        return new ConsoleInputNode { };
    }

    private static List<ParameterNode> ParseParameters()
    {
        try
        {
            Consume(TokenType.LeftParenthesis, $"Expected '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");

            List<ParameterNode> parameters = [];
            while (Peek().Type != TokenType.RightParenthesis)
            {
                if (Peek().Type == TokenType.Keyword)
                {
                    string type = Peek().Value;
                    Advance();
                    if (Peek().Type == TokenType.Identifier)
                    {
                        string name = Peek().Value;
                        var paramType = ParseDataType(type);
                        if (paramType == DataType.Void)
                            throw new CompilerError("A paramter cannot be of type void.");
                        parameters.Add(new ParameterNode { Name = name, Type = paramType });
                    }
                }
                Advance();
            }

            Consume(TokenType.RightParenthesis, $"Expected ')' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            return parameters;
        }
        catch (CompilerError ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(0);
            return null;
        }
    }

    private static StatementNode ParseMethodDeclaration()
    {
        Token returnType = Advance();
        var type = ParseDataType(returnType.Value);

        Token nameToken = Advance();
        if (nameToken.Type != TokenType.Identifier)
            throw new CompilerError($"Expected a name for method at line: {nameToken.LineNumber}.");
        string name = nameToken.Value;

        var @params = ParseParameters();

        var body = (BlockNode)ParseBlock();

        foreach (var stmt in body.Statements)
        {
            if (stmt is ReturnNode @return)
            {
                if (@return.ReturnType != type)
                    throw new CompilerError($"Cannot implicitly convert type '{@return.ReturnType}' to '{type}'");
            }
        }

        return new MethodDeclarationNode
        {
            Name = name,
            ReturnType = type,
            Parameters = [.. @params],
            Body = body
        };
    }

    private static StatementNode ParseReturn()
    {
        Consume(TokenType.Return);
        ExpressionNode value = null;
        if (Peek().Type != TokenType.Semicolon)
            value = ParseExpression();
        else
            value = new LiteralNode { Type = DataType.Void };
        Consume(TokenType.Semicolon, $"Expected ';' at line: {Peek().LineNumber - 1}.");
        return new ReturnNode
        {
            ReturnType = value.Type,
            Value = value
        };
    }

    private static StatementNode ParseMethodCall()
    {
        var tokenName = Peek();
        string name = tokenName.Value;
        Consume(TokenType.Identifier);

        Method method = null;
        if (methods.Peek().ContainsKey(name))
            method = methods.Peek()[name];
        else
            throw new CompilerError($"The name '{name}' doesn't exist in this scope.");
        Consume(TokenType.LeftParenthesis, $"Expected '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
        List<object> args = [];
        while (Peek().Type != TokenType.RightParenthesis)
        {
            Token argToken = Peek();
            var argType = ParseDataType(argToken.Type.ToString());
            args.Add(argToken.Value);
            Advance();
        }
        if (args.Count > method.Parameters.Length)
            throw new CompilerError("");
        if (args.Count < method.Parameters.Length)
            throw new CompilerError("");
        Consume(TokenType.RightParenthesis, $"Expected ')' at line: {Peek().LineNumber - 1}");
        Consume(TokenType.Semicolon, $"Expected ';' at line: {Peek().LineNumber}");
        return new MethodCallNode
        {
            Name = name,
            ReturnType = method.ReturnType,
            Arguments = []
        };
    }

    private static void GatherMethodDeclarations()
    {
        methods.Push([]);
        for (int i = 0; i < tokens.Count; i++)
        {
            switch (tokens[i].Type)
            {
                case TokenType.Keyword:
                    DataType type = ParseDataType(tokens[i].Value);
                    i++;
                    if (tokens[i].Type == TokenType.Identifier &&
                        tokens[i + 1].Type == TokenType.LeftParenthesis)
                    {
                        string name = tokens[i].Value;
                        i++;

                        if (tokens[i].Type != TokenType.LeftParenthesis)
                            throw new CompilerError($"Expected '(' at line: {tokens[i].LineNumber}, column: {tokens[i].ColumnNumber}.");

                        List<Parameter> parameters = [];
                        while (tokens[i].Type != TokenType.RightParenthesis)
                        {
                            if (tokens[i].Type == TokenType.Keyword)
                            {
                                string paramType = tokens[i].Value;
                                i++;
                                if (tokens[i].Type == TokenType.Identifier)
                                {
                                    string paramName = tokens[i].Value;
                                    var paramDateType = ParseDataType(paramType);
                                    if (paramDateType == DataType.Void)
                                        throw new CompilerError("A paramter cannot be of type void.");
                                    parameters.Add(new Parameter { Type = paramDateType, Name = paramName });
                                }
                            }
                            i++;
                        }

                        if (tokens[i].Type != TokenType.RightParenthesis)
                            throw new CompilerError($"Expected ')' at line: {tokens[i].LineNumber - 1}");
                    
                        methods.Peek().Add(name, new Method
                        {
                            Name = name,
                            Parameters = [.. parameters],
                            ReturnType = type,
                        });
                    }
                    break;
            }
        }
    }

}