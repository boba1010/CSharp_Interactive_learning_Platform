using DamageCalculator.Core;
using DamageCalculator.Models;
using DamageCalculator.Utils;

namespace DamageCalculator
{
    public class DamageCalculatorExecutable
    {
        private static readonly Stack<Dictionary<string, Variable>> variables = [];
        private static readonly Stack<Dictionary<string, Method>> methods = [];
        private static List<Token> tokens = [];
        private static int current = 0;

        private static readonly int syntaxErrorDamage = 5;
        private static readonly int typeErrorDamage = 8;
        private static readonly int ScopeErrorDamage = 3;
        private static readonly int expressionErrorDamage = 6;

        private static int totalDamage = 0;
        private static int totalSelfDamage = 0;

        private static readonly List<string> errMsgs = [];

        // CalculateDamage.exe "CODE" "IS_BOSS" "NUMBER_Of_ENEMIES" "HEALTH_PER_ENEMY"
        public static int Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Please enter the code, is a boss, the enemies number and the health for each enemy as the arguements. CalculateDamage.exe \"CODE\" 'IS_BOSS' \"NUMBER_OF_ENEMIES\"");
                return 1;
            }


            bool isBoss = bool.Parse(args[1]);
            int enemiesNumber = int.Parse(args[2]);
            int healthPerEnemy = int.Parse(args[3]);

            string codeText = args[0];
            tokens = Tokenizer.Tokenize(codeText);

            if (!tokens.Contains(new(TokenType.LeftBrace, "{")))
                variables.Push([]);

            ParseUsingDirective();

            GatherMethodDeclarations();

            List<StatementNode> statements = [];

            for (int i = 0; i < tokens.Count; i++)
            {
                var statement = ParseStatement();
                if (statement != null)
                    statements.Add(statement);
            }

            foreach (StatementNode statement in statements)
            {
                switch (statement)
                {
                    case DeclarationNode:
                        totalDamage += 5;
                        break;
                    case AssignementNode:
                        totalDamage += 2;
                        break;
                    case IfNode:
                        totalDamage += 8;
                        break;
                    case ElseIfNode:
                        totalDamage += 10;
                        break;
                    case WhileNode:
                        totalDamage += 6;
                        break;
                    case DoWhileNode:
                        totalDamage += 8;
                        break;
                    case SwitchNode:
                        totalDamage += 12;
                        break;
                    case BlockNode:
                        totalDamage += 12;
                        break;
                    case BreakNode:
                        totalDamage += 1;
                        break;
                    case ForNode:
                        totalDamage += 8;
                        break;
                    case MethodCallNode:
                        totalDamage += 15;
                        break;
                    case MethodDeclarationNode:
                        totalDamage += 17;
                        break;
                }
            }

            Console.WriteLine($"Errors: {string.Join('\n', errMsgs)} | Self Damage: {totalSelfDamage} | Damage Dealt: {totalDamage}");

            List<Enemy> enemies = [];
            for (int i = 0; i < enemiesNumber; i++)
            {
                var enemy = new Enemy() { Health = healthPerEnemy - totalDamage };
                if (totalDamage >= 0)
                    totalDamage -= healthPerEnemy;
                if (enemy.Health > 0)
                    break;
                enemies.Add(enemy);

            }

            Console.WriteLine(enemies.Count);
            return 0;
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
            return Peek().Type == type ? Advance() : new Token(TokenType.Damage, errMsg);
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
            var leftBrace = Consume(TokenType.LeftBrace, $"You forgot a '{{' at line: {Peek().LineNumber - 1}.");
            if (leftBrace.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(leftBrace.Value + "\n");
            }
            PushVariableScope();

            while (!Match(TokenType.RightBrace))
            {
                var statement = ParseStatement();

                if (statement != null)
                    statements.Add(statement);
            }

            var rightBrace = Consume(TokenType.RightBrace, $"You forgot a '}}' at line: {Peek().LineNumber - 1}.");
            if (rightBrace.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(rightBrace.Value + "\n");
            }
            PopVariableScope();

            return new BlockNode
            {
                Statements = statements
            };
        }

        private static StatementNode ParseStatement()
        {
            Token currentToken = Peek();
            if (currentToken.Type == TokenType.EOF)
                return null;
            try
            {
                StatementNode statement;
                switch (currentToken.Type)
                {
                    case TokenType.Keyword:
                        if (Match(TokenType.Identifier, 1) &&
                            Match(TokenType.LeftParenthesis, 2))
                            return ParseMethodDeclaration();
                        return ParseDeclaration();
                    case TokenType.Identifier:
                        if (Match(TokenType.LeftParenthesis, 1))
                            return ParseMethodCall();
                        return ParseAssignment();
                    case TokenType.If:
                        return ParseIf();
                    case TokenType.While:
                        return ParseWhile();
                    case TokenType.For:
                        return ParseFor();
                    case TokenType.Console:
                        return ParseConsoleMethod();
                    case TokenType.LeftBrace:
                        return ParseBlock();
                    case TokenType.Return:
                        return ParseReturn();
                    case TokenType.Break:
                        return ParseBreak();
                    case TokenType.Switch:
                        return ParseSwitch();
                    case TokenType.Do:
                        return ParseDoWhile();
                    default:
                        errMsgs.Add($"Unsupported token '{currentToken.Value}' at line: {currentToken.LineNumber}, column: {currentToken.ColumnNumber}.");
                        return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
                return null;
            }
        }

        private static void AddVariable(Variable @var)
        {
            var currentScope = variables.Peek();

            if (currentScope.ContainsKey(var.Name))
            {
                totalSelfDamage += ScopeErrorDamage;
                errMsgs.Add($"A local variable named '{var.Name}' is already defined in this scope at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.\n");
                return;
            }

            currentScope.Add(var.Name, new Variable
            {
                Name = var.Name,
                Type = var.Type,
                Value = var.Value,
            });
        }

        private static StatementNode ParseDeclaration()
        {
            Token typeToken = Consume(TokenType.Keyword, $"You can't declare a variable without a date type, fix it at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            if (typeToken.Type == TokenType.Damage)
            {
                totalSelfDamage += typeErrorDamage;
                errMsgs.Add(typeToken.Value + "\n");
                return null;
            }

            DataType type = ParseDataType(typeToken.Value); // convert string to enum
            if (type == DataType.Void)
            {
                totalSelfDamage += typeErrorDamage;
                errMsgs.Add("A variable cannot be of type void.\n");
                return null;
            }

            Token nameToken = Consume(TokenType.Identifier, $"You need a variable name at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            if (nameToken.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(nameToken.Value + "\n");
                return null;
            }
            string name = nameToken.Value;

            ExpressionNode expr = null;
            if (Peek().Type == TokenType.Equals)
            {
                Token equalToken = Consume(TokenType.Equals, $"You forgot a '=' after the variable name at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
                if (equalToken.Type == TokenType.Damage)
                {
                    totalSelfDamage += syntaxErrorDamage;
                    errMsgs.Add(equalToken.Value + "\n");
                    return null;
                }

                expr = ParseExpression();
                if (expr is LiteralNode ln)
                {
                    if (type != ln.Type)
                    {
                        totalSelfDamage += typeErrorDamage;
                        errMsgs.Add($"You can't assign type '{ln.Type.ToString().ToLower()}' to '{type.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.\n");
                        return null;
                    }
                }
                else if (expr is MethodCallExpressionNode methodCallExpression)
                {
                    if (methodCallExpression.Type != type)
                    {
                        totalSelfDamage += typeErrorDamage;
                        errMsgs.Add($"You can't assign type '{methodCallExpression.Type.ToString().ToLower()}' to '{type.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.\n");
                        return null;
                    }
                }
                else if (expr is VariableNode variable)
                {
                    if (variables.Peek().ContainsKey(variable.Name))
                    {
                        if (variables.Peek()[variable.Name].Type != type)
                        {
                            totalSelfDamage += typeErrorDamage;
                            errMsgs.Add($"You can't assign type '{variables.Peek()[variable.Name].Type.ToString().ToLower()}' to '{type.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.\n");
                            return null;
                        }
                    }
                }
            }

            if (Peek().Type == TokenType.Int ||
                Peek().Type == TokenType.String ||
                Peek().Type == TokenType.Double ||
                Peek().Type == TokenType.Float ||
                Peek().Type == TokenType.Char ||
                Peek().Type == TokenType.Bool)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add("You must have an '=' before a literal.\n");
                return null;
            }

            var semiColon = Consume(TokenType.Semicolon, $"You forgot a ';' at end of statement at line: {Peek().LineNumber - 1}.");
            if (semiColon.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(semiColon.Value + "\n");
                return null;
            }

            AddVariable(new()
            {
                Name = name,
                Type = type,
                Value = expr,
            });
            return new DeclarationNode() { Expression = expr, Name = name, Type = type }; ;
        }

        private static AssignementNode ParseAssignment()
        {
            Token nameToken = null;
            nameToken = Consume(TokenType.Identifier, $"Invalid expression at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            if (nameToken.Type == TokenType.Damage)
            {
                errMsgs.Add(nameToken.Value + "\n");
                return null;
            }

            var currentScope = variables.Peek();
            Variable variable = null;
            try
            {
                variable = currentScope[nameToken.Value];
            }
            catch
            {
                foreach (var scope in variables)
                {
                    if (scope.ContainsKey(nameToken.Value))
                    {
                        try
                        {
                            variable = scope[nameToken.Value];
                        }
                        catch (KeyNotFoundException)
                        {
                            totalSelfDamage += ScopeErrorDamage;
                            errMsgs.Add($"Error: The name '{nameToken.Value}' doesn't exist in this scope at line: {nameToken.LineNumber}, column: {nameToken.ColumnNumber}.\n");
                            return null;
                        }
                    }
                }
            }

            ExpressionNode expr = null;
            DataType variableType = DataType.Void;
            if (Match(TokenType.PlusPlus))
            {
                var left = new VariableNode { Name = variable.Name, Type = variable.Type };
                expr = MakeBinaryExpression(left, TokenType.PlusPlus, new LiteralNode { Type = variable.Type, Value = 1 });
                Advance();
            }
            else if (Match(TokenType.MinusMinus))
            {
                var left = new VariableNode { Name = variable.Name, Type = variable.Type };
                expr = MakeBinaryExpression(left, TokenType.MinusMinus, new LiteralNode { Type = variable.Type, Value = 1 });
                Advance();
            }
            else if (Match(TokenType.PlusEquals))
            {
                Advance();
                var left = new VariableNode { Name = variable.Name, Type = variable.Type };
                expr = MakeBinaryExpression(left, TokenType.PlusEquals, new LiteralNode { Type = variable.Type, Value = Peek().Value });
                Advance();
            }
            else if (Match(TokenType.MinusEquals))
            {
                Advance();
                var left = new VariableNode { Name = variable.Name, Type = variable.Type };
                expr = MakeBinaryExpression(left, TokenType.MinusEquals, new LiteralNode { Type = variable.Type, Value = Peek().Value });
                Advance();
            }
            else if (Match(TokenType.StarEquals))
            {
                Advance();
                var left = new VariableNode { Name = variable.Name, Type = variable.Type };
                expr = MakeBinaryExpression(left, TokenType.StarEquals, new LiteralNode { Type = variable.Type, Value = Peek().Value });
                Advance();
            }
            else if (Match(TokenType.SlashEquals))
            {
                Advance();
                var left = new VariableNode { Name = variable.Name, Type = variable.Type };
                expr = MakeBinaryExpression(left, TokenType.SlashEquals, new LiteralNode { Type = variable.Type, Value = Peek().Value });
                Advance();
            }
            else
            {
                Consume(TokenType.Equals, $"You forgot an '=' after the variable name at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");

                expr = ParseExpression();
                variableType = variable.Type;

                if (expr is LiteralNode ln)
                {
                    if (variableType != ln.Type)
                    {
                        totalSelfDamage += typeErrorDamage;
                        errMsgs.Add($"You can't assign type '{ln.Type.ToString().ToLower()}' to '{variableType.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.\n");
                        return null;
                    }
                }
                else if (expr is MethodCallExpressionNode methodCallExpression)
                {
                    if (methodCallExpression.Type != variableType)
                    {
                        totalSelfDamage += typeErrorDamage;
                        errMsgs.Add($"You can't assign type '{methodCallExpression.Type.ToString().ToLower()}' to '{variableType.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.\n");
                        return null;
                    }
                }
                else if (expr is VariableNode variableNode)
                {
                    if (variables.Peek().ContainsKey(variable.Name))
                    {
                        if (variables.Peek()[variable.Name].Type != variableType)
                        {
                            totalSelfDamage += typeErrorDamage;
                            errMsgs.Add($"You can't assign type '{variables.Peek()[variable.Name].Type.ToString().ToLower()}' to '{variableType.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.\n");
                            return null;
                        }
                    }
                }
            }


            var semicolon = Consume(TokenType.Semicolon, $"You forgot a ';' at end of statement at line: {Peek().LineNumber - 1}.");
            if (semicolon.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(semicolon.Value + "\n");
                return null;
            }
            
            currentScope.Update(nameToken.Value, new()
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
            };
        }

        private static BinaryExpressionNode MakeBinaryExpression(ExpressionNode left, TokenType op, ExpressionNode right)
        {
            if (op == TokenType.Plus ||
                op == TokenType.PlusEquals)
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
                errMsgs.Add($"Invalid types for '+' operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.\n");
            }
            if (op == TokenType.PlusPlus)
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
                errMsgs.Add($"Invalid types for '+' operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            }
            if (op == TokenType.Minus ||
                op == TokenType.MinusMinus ||
                op == TokenType.MinusEquals)
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
                errMsgs.Add($"Invalid types for '-' operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            }
            if (op == TokenType.Star ||
                op == TokenType.StarEquals)
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
                errMsgs.Add($"Invalid types for '*' operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            }
            if (op == TokenType.Slash ||
                op == TokenType.SlashEquals)
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
                errMsgs.Add($"Invalid types for '/' operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            }
            errMsgs.Add($"Invalid operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            return null;
        }

        // first
        private static ExpressionNode ParsePrimary()
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
            if (Match(TokenType.Identifier))
            {
                if (Match(TokenType.LeftParenthesis, 1))
                    return (MethodCallExpressionNode)ParseMethodCall(true);

                return new VariableNode { Type = ParseDataType(Peek().Type.ToString().ToLower(), Peek().Value), Name = Advance().Value };
            }
            if (Match(TokenType.LeftParenthesis))
            {
                Consume(TokenType.LeftParenthesis, "You forgot a '('");
                var expr = ParseExpression();
                Consume(TokenType.RightParenthesis, "You forgot a ')'");
                return expr;
            }

            errMsgs.Add($"Expected expression at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.\n");
            return null;
        }

        // above primary
        // second
        // handles '-', '!'
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
                expr = new BinaryExpressionNode { Left = expr, Operator = op.Value, Right = right, Type = expr.Type };
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
                expr = new BinaryExpressionNode { Left = expr, Operator = op.Value, Right = right, Type = expr.Type };
            }
            //Advance();
            return expr;
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
                catch
                {
                    foreach (var scope in variables)
                    {
                        if (scope.ContainsKey(identifier))
                        {
                            v = scope[identifier];
                        }
                    }
                }
                if (v == null)
                {
                    totalSelfDamage += ScopeErrorDamage;
                    errMsgs.Add($"The name '{identifier}' doesn't exist in this scope at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.\n");
                }

                var dataType = ParseDataType(v.Type.ToString().ToLower());
                return dataType;
            }

            DataType type = DataType.Error;

            switch (typeStr)
            {
                case "void":
                    type = DataType.Void;
                    break;
                case "int":
                    type = DataType.Int;
                    break;
                case "float": 
                    type = DataType.Float; 
                    break;
                case "bool":
                    type = DataType.Bool;
                    break;
                case "char":
                    type = DataType.Char;
                    break;
                case "string":
                    type = DataType.String;
                    break;
                default:
                    errMsgs.Add($"Unknown type: '{typeStr}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
                    break;
            }

            return type;
        }

        private static IfNode ParseIf()
        {
            Consume(TokenType.If);
            var leftParen = Consume(TokenType.LeftParenthesis, $"Expected '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            if (leftParen.Type == TokenType.Damage)
            {
                errMsgs.Add(leftParen.Value + "\n");
                totalSelfDamage += syntaxErrorDamage;
            }
            
            var expr = ParseExpression();
            expr.Type = DataType.Bool;
            var rightParen = Consume(TokenType.RightParenthesis, $"Expected ')' at line: {Peek().LineNumber - 1}");
            if (rightParen.Type == TokenType.Damage)
            {
                errMsgs.Add(rightParen.Value + "\n");
                totalSelfDamage += syntaxErrorDamage;
            }

            var thenStatements = ParseStatement();

            StatementNode elseStatements = null;
            List<ElseIfNode> elifs = [];
            while (Match(TokenType.Else) && Match(TokenType.If, 1))
            {
                current += 2;
                var elifLeftParen = Consume(TokenType.LeftParenthesis, $"Expected '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
                if (elifLeftParen.Type == TokenType.Damage)
                {
                    errMsgs.Add(elifLeftParen.Value + "\n");
                    totalSelfDamage += syntaxErrorDamage;
                }
                var elifExpr = ParseExpression();
                elifExpr.Type = DataType.Bool;
                var elifRightParen = Consume(TokenType.RightParenthesis, $"Expected ')' at line: {Peek().LineNumber - 1}");
                if (elifRightParen.Type == TokenType.Damage)
                {
                    errMsgs.Add(elifRightParen.Value + "\n");
                    totalSelfDamage += syntaxErrorDamage;
                }
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

        private static WhileNode ParseWhile()
        {
            Consume(TokenType.While);
            var leftParen = Consume(TokenType.LeftParenthesis, $"Expected '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            if (leftParen.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(leftParen.Value + "\n");
                return null;
            }
            var expr = ParseExpression();
            expr.Type = DataType.Bool;
            var rightParen = Consume(TokenType.RightParenthesis, $"Expected ')' at line: {Peek().LineNumber - 1}");
            if (rightParen.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(rightParen.Value + "\n");
                return null;
            }

            StatementNode body = ParseStatement();

            return new WhileNode
            {
                Body = body,
                Condition = expr
            };
        }

        private static DoWhileNode ParseDoWhile()
        {
            Consume(TokenType.Do);
            var body = ParseStatement();

            var @while = Consume(TokenType.While, "The 'do' keyword must be followed by a 'while' keyword.");
            if (@while.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(@while.Value + "\n");
                return null;
            }

            var leftParen = Consume(TokenType.LeftParenthesis, $"Expected '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            if (leftParen.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(leftParen.Value + "\n");
                return null;
            }

            var expr = ParseExpression();
            var rightParen = Consume(TokenType.RightParenthesis, $"Expected ')' at line: {Peek().LineNumber - 1}");
            if (rightParen.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(rightParen.Value + "\n");
                return null;
            }

            var semicolon = Consume(TokenType.Semicolon, "Expected ';'");
            if (semicolon.Type == TokenType.Damage)
            {
                totalDamage += syntaxErrorDamage;
                errMsgs.Add(semicolon.Value + "\n");
                return null;
            }

            return new DoWhileNode
            {
                Body = (BlockNode)body,
                Condition = expr
            };
        }

        private static BlockNode ParseForBlock()
        {
            List<StatementNode> statements = [];

            var leftBrace = Consume(TokenType.LeftBrace, $"You forgot a '{{' at line: {Peek().LineNumber - 1}.");
            if (leftBrace.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(leftBrace.Value + "\n");
                return null;
            }

            while (!Match(TokenType.RightBrace))
            {
                var statement = ParseStatement();
                
                if (statement != null)
                    statements.Add(statement);
            }

            var rightBrace = Consume(TokenType.RightBrace, $"You forgot a '}}' at line: {Peek().LineNumber - 1}.");
            if (rightBrace.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(rightBrace.Value + "\n");
                return null;
            }

            return new BlockNode { Statements = statements };
        }
        private static ForNode ParseFor()
        {
            Consume(TokenType.For);

            var leftParen = Consume(TokenType.LeftParenthesis, $"Expected '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            if (leftParen.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(leftParen.Value);
                return null;
            }

            PushVariableScope();

            // for (*int i = 0;* i < length;)
            DeclarationNode startPoint = (DeclarationNode)ParseDeclaration();

            // for (int i = 0; *i < length;*)
            var expr = ParseExpression();
            expr.Type = DataType.Bool;
            var semicolon = Consume(TokenType.Semicolon, $"Expected ';' at line: {Peek().LineNumber - 1}.");
            if (semicolon.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage; 
                errMsgs.Add(semicolon.Value + "\n");
                return null;
            }

            // for (int i = 0; i < length; *i++*)
            AssignementNode steps = null;
            if (Match(TokenType.Identifier))
            {
                Advance();
                if (Match(TokenType.PlusPlus))
                {
                    steps = new AssignementNode
                    {
                        Type = startPoint.Type,
                        Name = startPoint.Name,
                        Expression = new UnaryExpressionNode
                        {
                            Operator = "++"
                        }
                    };
                }
                else if (Match(TokenType.MinusMinus))
                {
                    steps = new AssignementNode
                    {
                        Type = startPoint.Type,
                        Name = startPoint.Name,
                        Expression = new UnaryExpressionNode
                        {
                            Operator = "--"
                        }
                    };
                }
                else if (Match(TokenType.PlusEquals))
                {
                    Advance();
                    steps = new AssignementNode
                    {
                        Type = startPoint.Type,
                        Name = startPoint.Name,
                        Expression = new BinaryExpressionNode
                        {
                            Operator = "+",
                            Left = new VariableNode
                            {
                                Name = startPoint.Name,
                                Type = startPoint.Type
                            },
                            Right = ParseExpression()
                        }
                    };
                }
                else if (Match(TokenType.MinusEquals))
                {
                    Advance();
                    steps = new AssignementNode
                    {
                        Type = startPoint.Type,
                        Name = startPoint.Name,
                        Expression = new BinaryExpressionNode
                        {
                            Operator = "-",
                            Left = new VariableNode
                            {
                                Name = startPoint.Name,
                                Type = startPoint.Type
                            },
                            Right = ParseExpression()
                        }
                    };
                }
                else if (Match(TokenType.SlashEquals))
                {
                    Advance();
                    steps = new AssignementNode
                    {
                        Type = startPoint.Type,
                        Name = startPoint.Name,
                        Expression = new BinaryExpressionNode
                        {
                            Operator = "/",
                            Left = new VariableNode
                            {
                                Name = startPoint.Name,
                                Type = startPoint.Type
                            },
                            Right = ParseExpression()
                        }
                    };
                }
                else if (Match(TokenType.StarEquals))
                {
                    Advance();
                    steps = new AssignementNode
                    {
                        Type = startPoint.Type,
                        Name = startPoint.Name,
                        Expression = new BinaryExpressionNode
                        {
                            Operator = "*",
                            Left = new VariableNode
                            {
                                Name = startPoint.Name,
                                Type = startPoint.Type
                            },
                            Right = ParseExpression()
                        }
                    };
                }
            }

            var rightParen = Consume(TokenType.RightParenthesis, $"Expected ')' at line: {Peek().LineNumber - 1}");
            if (rightParen.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(rightParen.Value + "\n");
                return null;
            }

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

        private static ConsoleNode ParseConsoleMethod()
        {
            Consume(TokenType.Console);
            Consume(TokenType.Dot);
            if (Match(TokenType.ConsoleOutput))
                return ParseConsoleOutputMethod();
            if (Match(TokenType.ConsoleInput))
                return ParseConsoleInputMethod();

            errMsgs.Add("Invalid Console method.");
            return null;
        }

        private static ConsoleOutputNode ParseConsoleOutputMethod()
        {
            Consume(TokenType.ConsoleOutput);

            var leftParen = Consume(TokenType.LeftParenthesis, $"Expected '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            if (leftParen.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(leftParen.Value + "\n");
                return null;
            }

            if (Peek().Type == TokenType.RightParenthesis)
            {
                Advance();

                var semiColon = Consume(TokenType.Semicolon, "Expected ';'");
                if (semiColon.Type == TokenType.Damage)
                {
                    totalDamage += syntaxErrorDamage;
                    errMsgs.Add(semiColon.Value = "\n");
                    return null;
                }
                return new ConsoleOutputNode { Object = new LiteralNode { Value = '\n' } };
            }
            var expr = ParseExpression();

            var rightParen = Consume(TokenType.RightParenthesis, $"Expected ')' at line: {Peek().LineNumber - 1}");
            if (rightParen.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(rightParen.Value + "\n");
                return null;
            }

            var semicolon = Consume(TokenType.Semicolon, "Expected ';'");
            if (semicolon.Type == TokenType.Damage)
            {
                totalDamage += syntaxErrorDamage;
                errMsgs.Add(semicolon.Value + "\n");
                return null;
            }

            return new ConsoleOutputNode { Object = expr };
        }

        private static ConsoleInputNode ParseConsoleInputMethod()
        {
            Consume(TokenType.ConsoleInput);

            var leftParen = Consume(TokenType.LeftParenthesis, $"Expected '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            if (leftParen.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(leftParen.Value + "\n");
                return null;
            }

            if (Peek().Type == TokenType.RightParenthesis)
            {
                Advance();
                var semiColon = Consume(TokenType.Semicolon, "Expected ';'");
                if (semiColon.Type == TokenType.Damage)
                {
                    totalDamage += syntaxErrorDamage;
                    errMsgs.Add(semiColon.Value + "\n");
                    return null;
                }

                return new ConsoleInputNode { };
            }

            var rightParen = Consume(TokenType.RightParenthesis, $"Expected ')' at line: {Peek().LineNumber - 1}");
            if (rightParen.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(rightParen.Value + "\n");
                return null;
            }

            var semicolon = Consume(TokenType.Semicolon, "Expected ';'");
            if (semicolon.Type == TokenType.Damage)
            {
                totalDamage += syntaxErrorDamage;
                errMsgs.Add(semicolon.Value + "\n");
                return null;
            }

            return new ConsoleInputNode { };
        }

        private static List<ParameterNode> ParseParameters()
        {
            var leftParen = Consume(TokenType.LeftParenthesis, $"You forgot a '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            if (leftParen.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(leftParen.Value + "\n");
                return null;
            }

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
                        {
                            errMsgs.Add("A paramter cannot be of type void.\n");
                            totalSelfDamage += typeErrorDamage;
                            return null;
                        }
                        parameters.Add(new ParameterNode { Name = name, Type = paramType });
                    }
                }
                Advance();
            }

            var rightParen = Consume(TokenType.RightParenthesis, $"You forgot a ')' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            if (rightParen.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(rightParen.Value + "\n");
                return null;
            }

            return parameters;
        }

        private static StatementNode ParseMethodDeclaration()
        {
            Token returnType = Advance();
            var type = ParseDataType(returnType.Value);

            Token nameToken = Advance();
            if (nameToken.Type != TokenType.Identifier)
                errMsgs.Add($"Expected a name for method at line: {nameToken.LineNumber}.\n");
            string name = nameToken.Value;

            var @params = ParseParameters();

            var body = (BlockNode)ParseBlock();

            ReturnNode returnNode = null;
            foreach (var stmt in body.Statements)
            {
                if (stmt is ReturnNode @return)
                {
                    if (@return.ReturnType != type)
                    {
                        totalSelfDamage += typeErrorDamage;
                        errMsgs.Add($"You can't return a/an '{@return.ReturnType}' while this method's return type is '{type}'\n");
                        return null;
                    }
                    returnNode = @return;
                }
            }

            if (type != DataType.Void)
            {
                if (returnNode == null)
                {
                    totalSelfDamage += typeErrorDamage;
                    errMsgs.Add($"Method '{name}' should return a value while it is not of type void.\n");
                    return null;
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

        private static ReturnNode ParseReturn()
        {
            Consume(TokenType.Return);
            ExpressionNode value = null;
            if (Peek().Type != TokenType.Semicolon)
                value = ParseExpression();
            else
                value = new LiteralNode { Type = DataType.Void };

            var semicolon = Consume(TokenType.Semicolon, "Expected ';'");
            if (semicolon.Type == TokenType.Damage)
            {
                totalDamage += syntaxErrorDamage;
                errMsgs.Add(semicolon.Value);
                return null;
            }

            return new ReturnNode
            {
                ReturnType = value.Type,
                Value = value
            };
        }

        private static MethodCallNode ParseMethodCall(bool isInExpression = false)
        {
            var tokenName = Peek();
            string name = tokenName.Value;
            Consume(TokenType.Identifier);

            Method method = null;
            if (methods.Peek().ContainsKey(name))
                method = methods.Peek()[name];
            else
            {
                totalSelfDamage += ScopeErrorDamage;
                errMsgs.Add($"The name '{name}' doesn't exist in this scope.\n");
                return null;
            }

            var leftParen = Consume(TokenType.LeftParenthesis, $"You forgot a '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            if (leftParen.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(leftParen.Value + "\n");
                return null;
            }

            List<object> args = [];
            List<DataType> argsType = [];
            while (Peek().Type != TokenType.RightParenthesis)
            {
                Token argToken = Peek();
                var argType = ParseDataType(argToken.Type.ToString().ToLower());
                argsType.Add(argType);
                args.Add(argToken.Value);
                Advance();
            }

            if (args.Count < method.Parameters.Length)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add("There is no arguments given to this method.\n");
                return null;
            }
            else if (args.Count > method.Parameters.Length)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add($"This method takes only {method.Parameters.Length} argument.\n");
                return null;
            }

            var parameters = method.Parameters;
            for (int i = 0; i < method.Parameters.Length; i++)
            {
                if (parameters[i].Type != argsType[i])
                {
                    totalSelfDamage += typeErrorDamage;
                    errMsgs.Add($"You can't give this method arguement of type '{argsType[i].ToString().ToLower()}' while the parameter is '{parameters[i].Type.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}\n");
                    return null;
                }
            }

            var rightParen = Consume(TokenType.RightParenthesis, $"You forgot a ')' at line: {Peek().LineNumber - 1}");
            if (rightParen.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(rightParen.Value + "\n");
                return null;
            }

            if (!isInExpression)
            {
                var semicolon = Consume(TokenType.Semicolon, $"You forgot a ';' at line: {Peek().LineNumber}");
                if (semicolon.Type == TokenType.Damage)
                {
                    totalSelfDamage += syntaxErrorDamage;
                    errMsgs.Add(semicolon.Value + "\n");
                    return null;
                }
            }

            return new MethodCallNode
            {
                Name = name,
                ReturnType = method.ReturnType,
                Arguments = [.. args]
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
                            {
                                totalSelfDamage += syntaxErrorDamage;
                                errMsgs.Add($"You forgot a '(' at line: {tokens[i].LineNumber}, column: {tokens[i].ColumnNumber}.\n");
                                return;
                            }

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
                                        {
                                            totalSelfDamage += typeErrorDamage;
                                            errMsgs.Add("A paramter cannot be of type void.\n");
                                            return;
                                        }
                                        parameters.Add(new Parameter { Type = paramDateType, Name = paramName });
                                    }
                                }
                                i++;
                            }

                            if (tokens[i].Type != TokenType.RightParenthesis)
                            {
                                totalSelfDamage += syntaxErrorDamage;
                                errMsgs.Add($"You forgot a ')' at line: {tokens[i].LineNumber - 1}\n");
                                return;
                            }

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

        private static void ParseUsingDirective()
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                if (Match(TokenType.UsingDirective))
                {
                    Advance();
                    if (Peek().Value == "System" ||
                        Peek().Value == "System.Collections.Generic")
                        Advance();
                    else
                        errMsgs.Add("Security Error: Cannot use namespaces except 'System' and 'System.Collections.Generic'.\n");
                    var semicolon = Consume(TokenType.Semicolon, $"You forgot a semicolon at line: {Peek().LineNumber - 1}.");
                    if (semicolon.Type == TokenType.Damage)
                    {
                        errMsgs.Add(semicolon.Value + "\n");
                        totalSelfDamage += syntaxErrorDamage;
                        return;
                    }
                }
            }
        }

        private static BlockNode ParseCaseBody()
        {
            var colon = Consume(TokenType.Colon, $"You forgot a ':' at line: {Peek().LineNumber - 1}.");
            if (colon.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(colon.Value);
                return null;
            }

            if (Match(TokenType.Case))
                return new BlockNode { Statements = [] };

            List<StatementNode> statements = [];
            while (!Match(TokenType.RightBrace))
            {
                var statement = ParseStatement();
                if (statement != null)
                    statements.Add(statement);
            }
            return new BlockNode { Statements = statements };
        }

        private static CaseNode ParseCase()
        {
            Consume(TokenType.Case);
            var expr = ParseExpression();
            var body = ParseCaseBody();
            
            for (int i = 0; i < body.Statements.Count; i++)
            {
                if (body.Statements[i] is BreakNode ||
                    body.Statements[i] is ReturnNode)
                {
                    return new CaseNode
                    {
                        Body = body,
                        Condition = expr
                    };
                }

            }
            if (body.Statements.Count != 0)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add("A case with statement(s) should end with a 'break' or 'return'.\n");
                return null;
            }

            return new CaseNode
            {
                Body = body,
                Condition = expr
            };
        }

        private static BreakNode ParseBreak()
        {
            Consume(TokenType.Break);
            var semicolon = Consume(TokenType.Semicolon, "Expected ';'");
            if (semicolon.Type == TokenType.Damage)
            {
                totalDamage += syntaxErrorDamage;
                errMsgs.Add(semicolon.Value);
                return null;
            }

            return new BreakNode { };
        }

        private static SwitchBlockNode ParseSwitchBody()
        {
            List<CaseNode> cases = [];
            var leftBrace = Consume(TokenType.LeftBrace, $"You forgot a '{{' at line: {Peek().LineNumber - 1}.");
            if (leftBrace.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(leftBrace.Value + "\n");
                return null;
            }

            PushVariableScope();
            while (!Match(TokenType.RightBrace))
            {
                var @case = ParseCase();

                if (@case != null)
                    cases.Add(@case);
            }

            var rightBrace = Consume(TokenType.RightBrace, $"You forgot a '}}' at line: {Peek().LineNumber - 1}.");
            if (rightBrace.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(rightBrace.Value + "\n");
                return null;
            }

            PopVariableScope();

            return new SwitchBlockNode { Cases = [.. cases] };
        }

        private static SwitchNode ParseSwitch()
        {
            Consume(TokenType.Switch);
            var leftParen = Consume(TokenType.LeftParenthesis, $"Expected '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            if (leftParen.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(leftParen.Value);
                return null;
            }

            var expr = ParseExpression();
            var rightParen = Consume(TokenType.RightParenthesis, $"Expected ')' at line: {Peek().LineNumber - 1}");
            if (rightParen.Type == TokenType.Damage)
            {
                totalSelfDamage += syntaxErrorDamage;
                errMsgs.Add(rightParen.Value);
                return null;
            }

            var body = ParseSwitchBody();

            for (int i = 0; i < body.Cases.Length; i++)
            {
                if (i == body.Cases.Length - 1)
                {
                    for (int j = 0; j < body.Cases[i].Body.Statements.Count; j++)
                    {
                        if (body.Cases[i].Body.Statements[j] is ReturnNode ||
                            body.Cases[i].Body.Statements[j] is BreakNode)
                        {
                            return new SwitchNode
                            {
                                Body = body,
                                Expression = expr
                            };
                        }

                    }
                }
            }

            errMsgs.Add("The last case should end with 'return' or 'break'.\n");

            return null;
        }
    }
}
