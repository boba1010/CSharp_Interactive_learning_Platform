using DamageCalculatorV2.Core;
using DamageCalculatorV2.Exceptions;
using DamageCalculatorV2.Models;
using DamageCalculatorV2.Utils;

namespace DamageCalculatorV2
{
    public class DmgCalc
    {
        private Stack<Dictionary<string, Variable>> variables = [];
        private Stack<Dictionary<string, Method>> methods = [];
        private List<Token> tokens = [];
        private int current = 0;

        private const int syntaxErrorDamage = 5;
        private const int typeErrorDamage = 8;
        private const int ScopeErrorDamage = 12;
        private const int expressionErrorDamage = 6;

        private double totalDamage = 0;
        private double totalSelfDamage = 0;

        private readonly List<string> errMsgs = [];

        public CalculationResult Main(string code, int enemiesNumber, int healthPerEnemy, bool isBoss, double dmgMultiplier)
        {
            tokens = Tokenizer.Tokenize(code);

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
                else
                    break;
            }

            foreach (StatementNode statement in statements)
            {
                switch (statement)
                {
                    case DeclarationNode:
                        totalDamage += 5 * dmgMultiplier;
                        break;
                    case AssignementNode:
                        totalDamage += 2 * dmgMultiplier;
                        break;
                    case IfNode:
                        totalDamage += 8 * dmgMultiplier;
                        break;
                    case ElseIfNode:
                        totalDamage += 10 * dmgMultiplier;
                        break;
                    case WhileNode:
                        totalDamage += 6 * dmgMultiplier;
                        break;
                    case DoWhileNode:
                        totalDamage += 8 * dmgMultiplier;
                        break;
                    case SwitchNode:
                        totalDamage += 12 * dmgMultiplier;
                        break;
                    case BlockNode:
                        totalDamage += 12 * dmgMultiplier;
                        break;
                    case BreakNode:
                        totalDamage += 1 * dmgMultiplier;
                        break;
                    case ForNode:
                        totalDamage += 8 * dmgMultiplier;
                        break;
                    case MethodCallNode:
                        totalDamage += 15 * dmgMultiplier;
                        break;
                    case MethodDeclarationNode:
                        totalDamage += 17 * dmgMultiplier;
                        break;
                }
            }

            Console.WriteLine($"Self Damage: {totalSelfDamage} | Damage Dealt: {totalDamage}");

            List<Enemy> enemies = [];
            for (int i = 0; i < enemiesNumber; i++)
            {
                var enemy = new Enemy() { Health = (int)Math.Round(healthPerEnemy - totalDamage) };
                totalDamage -= healthPerEnemy;
                if (totalDamage < 0)
                    totalDamage = 0;
                if (enemy.Health <= 0)
                    continue;
                enemies.Add(enemy);

            }

            Console.WriteLine(string.Join('\n', errMsgs));

            return new()
            {
                Errors = errMsgs,
                RemainingEnemies = enemies,
                SelfDamage = (int)Math.Round(totalSelfDamage)
            };
        }

        private Token Peek(int index = 0)
        {
            if (current >= tokens.Count)
                return new(TokenType.EOF, "END", tokens[current - 1].LineNumber);
            return tokens[current + index];
        }

        private Token Advance()
        {
            if (current != tokens.Count)
                return tokens[current++];
            return tokens[^1];
        }

        private Token Consume(TokenType type, string errMsg)
        {
            while (Peek().Type == TokenType.Unknown)
                Advance();
            return Peek().Type == type ? Advance() : throw new SyntaxError($"{errMsg}");
        }
        private Token Consume(TokenType type)
        {
            while (Peek().Type == TokenType.Unknown)
                Advance();
            return Peek().Type == type ? Advance() : null;
        }

        private bool Match(TokenType type, int index = 0)
        {
            return Peek(index).Type == type;
        }

        public void PushVariableScope()
        {
            variables.Push([]);
        }

        public void PopVariableScope()
        {
            variables.Pop();
        }

        private BlockNode ParseBlock()
        {
            List<StatementNode> statements = [];
            Consume(TokenType.LeftBrace, $"You forgot a '{{' at line: {Peek().LineNumber - 1}.");
            PushVariableScope();
            while (!Match(TokenType.RightBrace))
            {
                var statement = ParseStatement();
                if (statement != null)
                    statements.Add(statement);
            }

            Consume(TokenType.RightBrace, $"You forgot a '}}' at line: {Peek().LineNumber - 1}.");
            PopVariableScope();
            return new BlockNode { Statements = statements };
        }

        private StatementNode ParseStatement()
        {
            Token currentToken = Peek();
            if (currentToken.Type == TokenType.EOF)
                return null;
            try
            {
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
                        return ParseConsoleMethodType();
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
                    case TokenType.Unknown:
                        Advance();
                        return null;
                    default:
                        throw new TokenError($"Unsupported token '{currentToken.Value}' at line: {currentToken.LineNumber}, column: {currentToken.ColumnNumber}.");
                }
            }
            catch (TokenError ex)
            {
                errMsgs.Add(ex.Message);
                totalSelfDamage += 3;
                return null;
            }
            catch (ScopeError ex)
            {
                errMsgs.Add(ex.Message);
                totalSelfDamage += ScopeErrorDamage;
                return null;
            }
            catch (SyntaxError ex)
            {
                errMsgs.Add(ex.Message);
                totalSelfDamage += syntaxErrorDamage;
                return null;
            }
            catch (ExpressionError ex)
            {
                errMsgs.Add(ex.Message);
                totalSelfDamage += expressionErrorDamage;
                return null;
            }
            catch (TypeError ex)
            {
                errMsgs.Add(ex.Message);
                totalSelfDamage += typeErrorDamage;
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(0);
                return null;
            }
        }

        private void AddVariable(Variable @var)
        {
            try
            {
                var currentScope = variables.Peek();

                if (currentScope.ContainsKey(var.Name))
                    throw new ScopeError($"A local variable named '{var.Name}' is already defined in this scope at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");

                currentScope.Add(var.Name, new Variable
                {
                    Name = var.Name,
                    Type = var.Type,
                    Value = var.Value,
                });
            }
            catch (ScopeError ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(0);
            }
        }

        private DeclarationNode ParseDeclaration()
        {
            Token typeToken = Consume(TokenType.Keyword, $"You can't declare a variable without a date type, fix it at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            DataType type = ParseDataType(typeToken.Value); // convert string to enum
            if (type == DataType.Void)
                throw new TypeError("A variable cannot be of type void.");
            Token nameToken = Consume(TokenType.Identifier, $"You need a variable name at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            string name = nameToken.Value;

            ExpressionNode expr = null;
            if (Peek().Type == TokenType.Equals)
            {
                Consume(TokenType.Equals, $"You forgot a '=' after the variable name at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");

                expr = ParseExpression();
                if (expr is LiteralNode ln)
                {
                    if (type != ln.Type)
                        throw new TypeError($"You can't assign type '{ln.Type.ToString().ToLower()}' to '{type.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
                }
                else if (expr is MethodCallExpressionNode methodCallExpression)
                {
                    if (methodCallExpression.Type != type)
                        throw new TypeError($"You can't assign type '{methodCallExpression.Type.ToString().ToLower()}' to '{type.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
                }
                else if (expr is VariableNode variable)
                {
                    if (variables.Peek().ContainsKey(variable.Name))
                    {
                        if (variables.Peek()[variable.Name].Type != type)
                            throw new TypeError($"You can't assign type '{variables.Peek()[variable.Name].Type.ToString().ToLower()}' to '{type.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
                    }
                }
            }

            if (Peek().Type == TokenType.Int ||
                Peek().Type == TokenType.String ||
                Peek().Type == TokenType.Double ||
                Peek().Type == TokenType.Float ||
                Peek().Type == TokenType.Char ||
                Peek().Type == TokenType.Bool)
                throw new SyntaxError("You must have an '=' before a literal.");

            Consume(TokenType.Semicolon, $"You forgot a ';' at end of statement at line: {Peek().LineNumber - 1}.");
            AddVariable(new()
            {
                Name = name,
                Type = type,
                Value = expr,
            });

            return new DeclarationNode { Name = name, Type = type, Expression = expr };
        }

        private AssignementNode ParseAssignment()
        {
            Token nameToken = null;
            try
            {
                nameToken = Consume(TokenType.Identifier, $"Invalid expression at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
                var currentScope = variables.Peek();
                Variable variable = new();
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
                            variable = scope[nameToken.Value];
                        }
                        else
                        {
                            throw new ScopeError($"Error: The name '{nameToken.Value}' doesn't exist in this scope at line: {nameToken.LineNumber}, column: {nameToken.ColumnNumber}.");
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
                            throw new TypeError($"You can't assign type '{ln.Type.ToString().ToLower()}' to '{variableType.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
                    }
                    else if (expr is MethodCallExpressionNode methodCallExpression)
                    {
                        if (methodCallExpression.Type != variableType)
                            throw new TypeError($"You can't assign type '{methodCallExpression.Type.ToString().ToLower()}' to '{variableType.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
                    }
                    else if (expr is VariableNode variableNode)
                    {
                        if (variables.Peek().ContainsKey(variable.Name))
                        {
                            if (variables.Peek()[variable.Name].Type != variableType)
                                throw new TypeError($"You can't assign type '{variables.Peek()[variable.Name].Type.ToString().ToLower()}' to '{variableType.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
                        }
                    }
                }


                Consume(TokenType.Semicolon, $"You forgot a ';' at end of statement at line: {Peek().LineNumber - 1}.");
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
            catch (KeyNotFoundException)
            {
                Console.WriteLine($"Error: The name '{nameToken.Value}' doesn't exist in this scope at line: {nameToken.LineNumber}, column: {nameToken.ColumnNumber}.");
                Environment.Exit(0);
                return null;
            }
        }

        private BinaryExpressionNode MakeBinaryExpression(ExpressionNode left, TokenType op, ExpressionNode right)
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
                throw new TypeError($"Invalid types for '+' operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
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
                throw new TypeError($"Invalid types for '+' operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
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
                throw new TypeError($"Invalid types for '-' operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
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
                throw new TypeError($"Invalid types for '*' operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
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
                throw new TypeError($"Invalid types for '/' operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            }
            throw new ExpressionError($"Invalid operator at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
        }

        // first
        private ExpressionNode ParsePrimary()
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
            if (Match(TokenType.Char)) return new LiteralNode { Type = DataType.Char, Value = Advance().Value };
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

            throw new ExpressionError($"Expected expression at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
        }

        // above primary
        // second
        // handles '-', '!'
        private ExpressionNode ParseUnary()
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
        private ExpressionNode ParseFactor()
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
        private ExpressionNode ParseTerm()
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
        private ExpressionNode ParseComparision()
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
        private ExpressionNode ParseEquality()
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
        private ExpressionNode ParseAnd()
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
        private ExpressionNode ParseOr()
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

        private ExpressionNode ParseExpression()
        {
            var or = ParseOr();
            return or;
        }

        private DataType ParseDataType(string typeStr, string identifier = "")
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
                    throw new ScopeError($"The name '{identifier}' doesn't exist in this scope at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
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
                _ => throw new TypeError($"Unknown type: '{typeStr}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.")
            };
        }

        private IfNode ParseIf()
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

        private WhileNode ParseWhile()
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

        private DoWhileNode ParseDoWhile()
        {
            Consume(TokenType.Do);
            var body = ParseStatement();
            Consume(TokenType.While, "The 'do' keyword must be followed by a 'while' keyword.");
            Consume(TokenType.LeftParenthesis, $"You forgot a '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            var expr = ParseExpression();
            Consume(TokenType.RightParenthesis, $"You forgot a ')' at line: {tokens[current - 1].LineNumber}, column: {tokens[current - 1].ColumnNumber}.");
            Consume(TokenType.Semicolon, $"You must have a ';' after the do while statement at line: {tokens[current - 1].LineNumber}, column: {tokens[current - 1].ColumnNumber}.");
            return new DoWhileNode
            {
                Body = (BlockNode)body,
                Condition = expr
            };
        }

        private BlockNode ParseForBlock()
        {
            List<StatementNode> statements = [];
            Consume(TokenType.LeftBrace, $"You forgot a '{{' at line: {Peek().LineNumber - 1}.");
            while (!Match(TokenType.RightBrace))
            {
                var statement = ParseStatement();
                if (statement != null)
                    statements.Add(statement);
            }

            Consume(TokenType.RightBrace, $"You forgot a '}}' at line: {Peek().LineNumber - 1}.");
            return new BlockNode { Statements = statements };
        }
        private ForNode ParseFor()
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

        private ConsoleNode ParseConsoleMethodType()
        {
            Consume(TokenType.Console);
            Consume(TokenType.Dot);
            if (Match(TokenType.ConsoleOutput))
                return ParseConsoleOutputMethod();
            if (Match(TokenType.ConsoleInput))
                return ParseConsoleInputMethod();
            throw new TokenError("Invalid Console method.");
        }

        private ConsoleOutputNode ParseConsoleOutputMethod()
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

        private ConsoleInputNode ParseConsoleInputMethod()
        {
            Consume(TokenType.ConsoleInput);
            Consume(TokenType.LeftParenthesis, $"You forgot a '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            if (Peek().Type == TokenType.RightParenthesis)
            {
                Advance();
                Consume(TokenType.Semicolon, $"You forgot a ';' at line: {Peek().LineNumber - 1}.");
                return new ConsoleInputNode { };
            }
            Consume(TokenType.RightParenthesis, $"You forgot a ')' at line: {Peek().LineNumber - 1}.");
            Consume(TokenType.Semicolon, $"You forgot a ';' at line: {Peek().LineNumber - 1}.");
            return new ConsoleInputNode { };
        }

        private List<ParameterNode> ParseParameters()
        {
            Consume(TokenType.LeftParenthesis, $"You forgot a '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");

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
                            throw new TypeError("A paramter cannot be of type void.");
                        parameters.Add(new ParameterNode { Name = name, Type = paramType });
                    }
                }
                Advance();
            }

            Consume(TokenType.RightParenthesis, $"You forgot a ')' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
            return parameters;
        }

        private MethodDeclarationNode ParseMethodDeclaration()
        {
            Token returnType = Advance();
            var type = ParseDataType(returnType.Value);

            Token nameToken = Advance();
            if (nameToken.Type != TokenType.Identifier)
                throw new SyntaxError($"Expected a name for method at line: {nameToken.LineNumber}.");
            string name = nameToken.Value;

            var @params = ParseParameters();

            var body = (BlockNode)ParseBlock();

            ReturnNode returnNode = null;
            foreach (var stmt in body.Statements)
            {
                if (stmt is ReturnNode @return)
                {
                    if (@return.ReturnType != type)
                        throw new TypeError($"You can't return a/an '{@return.ReturnType}' while this method's return type is '{type}'");
                    returnNode = @return;
                }
            }

            if (type != DataType.Void)
            {
                if (returnNode == null)
                    throw new TypeError($"Method '{name}' should return a value while it is not of type void.");
            }

            return new MethodDeclarationNode
            {
                Name = name,
                ReturnType = type,
                Parameters = [.. @params],
                Body = body
            };
        }

        private ReturnNode ParseReturn()
        {
            Consume(TokenType.Return);
            ExpressionNode value = null;
            if (Peek().Type != TokenType.Semicolon)
                value = ParseExpression();
            else
                value = new LiteralNode { Type = DataType.Void };
            Consume(TokenType.Semicolon, $"You forgot a ';' at line: {Peek().LineNumber - 1}.");
            return new ReturnNode
            {
                ReturnType = value.Type,
                Value = value
            };
        }

        private MethodCallNode ParseMethodCall(bool isInExpression = false)
        {
            var tokenName = Peek();
            string name = tokenName.Value;
            Consume(TokenType.Identifier);

            Method method = null;
            if (methods.Peek().ContainsKey(name))
                method = methods.Peek()[name];
            else
                throw new ScopeError($"The name '{name}' doesn't exist in this scope.");
            Consume(TokenType.LeftParenthesis, $"You forgot a '(' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}.");
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
                throw new SyntaxError("There is no arguments given to this method.");
            if (args.Count > method.Parameters.Length)
                throw new SyntaxError($"This method takes only {method.Parameters.Length} argument.");

            var parameters = method.Parameters;
            for (int i = 0; i < method.Parameters.Length; i++)
            {
                if (parameters[i].Type != argsType[i])
                    throw new TypeError($"You can't give this method arguement of type '{argsType[i].ToString().ToLower()}' while the parameter is '{parameters[i].Type.ToString().ToLower()}' at line: {Peek().LineNumber}, column: {Peek().ColumnNumber}");
            }

            Consume(TokenType.RightParenthesis, $"You forgot a ')' at line: {Peek().LineNumber - 1}");
            if (!isInExpression)
                Consume(TokenType.Semicolon, $"You forgot a ';' at line: {Peek().LineNumber}");
            return new MethodCallNode
            {
                Name = name,
                ReturnType = method.ReturnType,
                Arguments = [.. args]
            };
        }

        private void GatherMethodDeclarations()
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
                                throw new SyntaxError($"You forgot a '(' at line: {tokens[i].LineNumber}, column: {tokens[i].ColumnNumber}.");

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
                                            throw new TypeError("A paramter cannot be of type void.");
                                        parameters.Add(new Parameter { Type = paramDateType, Name = paramName });
                                    }
                                }
                                i++;
                            }

                            if (tokens[i].Type != TokenType.RightParenthesis)
                                throw new SyntaxError($"You forgot a ')' at line: {tokens[i].LineNumber - 1}");

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

        private void ParseUsingDirective()
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
                        throw new SecurityError("Security Error: Cannot use namespaces except 'System' and 'System.Collections.Generic'.");
                    Consume(TokenType.Semicolon, $"You forgot a semicolon at line: {Peek().LineNumber - 1}.");
                }
            }
        }

        private BlockNode ParseCaseBody()
        {
            Consume(TokenType.Colon, $"You forgot a ':' at line: {Peek().LineNumber - 1}.");

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

        private CaseNode ParseCase()
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
                throw new SyntaxError("A case with statement(s) should end with a 'break' or 'return'.");

            return new CaseNode
            {
                Body = body,
                Condition = expr
            };
        }

        private BreakNode ParseBreak()
        {
            Consume(TokenType.Break);
            Consume(TokenType.Semicolon, $"You forgot a ';' at line: {Peek().LineNumber}");
            return new BreakNode { };
        }

        private SwitchBlockNode ParseSwitchBody()
        {
            List<CaseNode> cases = [];
            Consume(TokenType.LeftBrace, $"You forgot a '{{' at line: {Peek().LineNumber - 1}.");
            PushVariableScope();
            while (!Match(TokenType.RightBrace))
            {
                var @case = ParseCase();
                if (@case != null)
                    cases.Add(@case);
            }

            Consume(TokenType.RightBrace, $"You forgot a '}}' at line: {Peek().LineNumber - 1}.");
            PopVariableScope();
            return new SwitchBlockNode { Cases = [.. cases] };
        }

        private SwitchNode ParseSwitch()
        {
            Consume(TokenType.Switch);
            Consume(TokenType.LeftParenthesis, $"You forgot a '(' at line: {Peek().LineNumber - 1}, column: {Peek().ColumnNumber}.");
            var expr = ParseExpression();
            Consume(TokenType.RightParenthesis, $"You forgot a ')' at line: {tokens[current - 1].LineNumber}, column: {tokens[current - 1].ColumnNumber}.");
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

            throw new SyntaxError("The last case should end with 'return' or 'break'.");
        }
    }
}