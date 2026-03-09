using CSharp_Interpreter.Exceptions;
using CSharp_Interpreter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp_Interpreter.Core
{
    public static class CodeGenerator
    {
        public static string GeneratePyCode(BlockNode block, int indent = 0, int innerIndent = 1)
        {
            // python indent
            string indentation = new(' ', indent * 4);

            var lines = new List<string>();
            foreach (var stmt in block.Statements)
            {
                switch (stmt)
                {
                    case DeclarationNode dcln:
                        lines.Add($"{indentation}{dcln.Name} = {GenerateExpression(dcln.Expression)}");
                        break;
                    case AssignementNode assign:
                        switch (assign.AssignmentType)
                        {
                            case AssignmentType.PlusPlus:
                                lines.Add($"{indentation}{assign.Name} += 1");
                                break;
                            case AssignmentType.MinusMinus:
                                lines.Add($"{indentation}{assign.Name} -= 1");
                                break;
                            default:
                                lines.Add($"{indentation}{assign.Name} {MapAssignmentTypes(assign.AssignmentType)} {GenerateExpression(assign.Expression)}");
                                break;
                        }
                        break;
                    case IfNode ifNode:
                        lines.Add($"{GenerateIfPyCode(ifNode, innerIndent)}");
                        break;
                    case WhileNode @while:
                        lines.Add($"{GenerateWhilePyCode(@while, innerIndent)}");
                        break;
                    case ForNode @for:
                        lines.Add(GenerateForPyCode(@for, innerIndent));
                        break;
                    case ConsoleNode console:
                        switch (console)
                        {
                            case ConsoleOutputNode output:
                                lines.Add($"print({GenerateExpression(output.Object)})");
                                break;
                            case ConsoleInputNode input:
                                lines.Add($"input()");
                                break;
                        }
                        break;
                    case MethodDeclarationNode methodDeclaration:
                        if (methodDeclaration.Name == "Main")
                            lines.Add(ResolveEntryPoint(methodDeclaration));
                        else
                            lines.Add($"{GenerateMethodDeclarationPyCode(methodDeclaration)}");
                        break;
                }
            }
            return string.Join('\n', lines);
        }

        private static string ResolveEntryPoint(MethodDeclarationNode methodDeclaration)
        {
            string indentation = new(' ', 4);
            var lines = new List<string>();
            if (methodDeclaration.Name == "Main" &&
                (methodDeclaration.ReturnType == DataType.Void ||
                 methodDeclaration.ReturnType == DataType.Int))
            {
                lines.Add("def Main():");
                lines.Add($"{indentation}{GeneratePyCode(methodDeclaration.Body, 1, 2)}");
                if (methodDeclaration.ReturnType == DataType.Void)
                    lines.Add($"{indentation}return");
                else
                {
                    foreach (var stmt in methodDeclaration.Body.Statements)
                    {
                        if (stmt is ReturnNode @return)
                            lines.Add($"{indentation}return {GenerateExpression(@return.Value)}");
                    }
                }
                lines.Add($"if __name__ == \"__main__\":\n\tMain()");
            }
            if (lines.Count == 0)
                return "";
            return string.Join('\n', lines);
        }

        private static string GenerateExpression(ExpressionNode expressionNode)
        {
            switch (expressionNode)
            {
                case UnaryExpressionNode unary:
                    return $"{Program.MapOperator(unary.Operator)} {GenerateExpression(unary.Operand)}";
                case LiteralNode ln:
                    if (ln.Type == DataType.String)
                        return $"{ln.Value}";
                    else if (ln.Type == DataType.Char)
                        return $"{ln.Value.ToString()[0]}";
                    else
                        return $"{ln.Value}";
                case VariableNode vn:
                    return vn.Name;
                case BinaryExpressionNode bexprn:
                    string left = null;
                    if (bexprn.Left is LiteralNode lLtn)
                    {
                        string right = null;
                        left = lLtn.Value.ToString();
                        if (bexprn.Right is BinaryExpressionNode bexprL)
                        {
                            if (bexprL.Type == lLtn.Type)
                                right = GenerateExpression(bexprL);
                            else
                            {
                                try
                                {
                                    throw new CompilerError($"Cannot implicitly convert type '{bexprn.Type}' to '{lLtn.Type}'");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error: {ex.Message}");
                                    Environment.Exit(1);
                                }
                            }
                        }
                        else if (bexprn.Right is VariableNode rightVn && bexprn.Right.Type == bexprn.Type)
                            right = rightVn.Name;
                        else if (bexprn.Right is LiteralNode rLn && bexprn.Right.Type == lLtn.Type)
                            right = rLn.Value.ToString();
                        return $"{left} {bexprn.Operator} {right}";
                    }
                    if (bexprn.Right is LiteralNode rLtn)
                    {
                        string right = null;
                        right = rLtn.Value.ToString();
                        if (bexprn.Left is BinaryExpressionNode bexprL)
                        {
                            if (bexprL.Type == rLtn.Type)
                                left = GenerateExpression(bexprL);
                            else
                            {
                                try
                                {
                                    throw new CompilerError($"Cannot implicitly convert type '{bexprn.Type}' to '{bexprL.Type}'");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error: {ex.Message}");
                                    Environment.Exit(1);
                                }
                            }
                        }
                        else if (bexprn.Left is VariableNode lVn && bexprn.Right.Type == rLtn.Type)
                            left = lVn.Name;
                        else if (bexprn.Left is LiteralNode lLn && bexprn.Right.Type == rLtn.Type)
                            left = lLn.Value.ToString();
                        return $"{left} {bexprn.Operator} {right}";
                    }
                    if (bexprn.Right is VariableNode rVn)
                    {
                        string right = null;
                        right = rVn.Name.ToString();
                        if (bexprn.Left is BinaryExpressionNode bexprL)
                        {
                            if (bexprL.Type == rVn.Type)
                                left = GenerateExpression(bexprL);
                            else
                            {
                                try
                                {
                                    throw new CompilerError($"Cannot implicitly convert type '{bexprn.Type}' to '{bexprL.Type}'");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error: {ex.Message}");
                                    Environment.Exit(1);
                                }
                            }
                        }
                        else if (bexprn.Left is VariableNode lVn && bexprn.Right.Type == rVn.Type)
                            left = lVn.Name;
                        else if (bexprn.Left is LiteralNode lLn && bexprn.Right.Type == rVn.Type)
                            left = lLn.Value.ToString();
                        return $"{left} {bexprn.Operator} {right}";
                    }
                    if (bexprn.Left is VariableNode leftVn)
                    {
                        string right = null;
                        right = leftVn.Name.ToString();
                        if (bexprn.Right is BinaryExpressionNode bexprL)
                        {
                            if (bexprL.Type == leftVn.Type)
                                right = GenerateExpression(bexprL);
                            else
                            {
                                try
                                {
                                    throw new CompilerError($"Cannot implicitly convert type '{bexprn.Type}' to '{leftVn.Type}'");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error: {ex.Message}");
                                    Environment.Exit(1);
                                }
                            }
                        }
                        else if (bexprn.Right is VariableNode rV && bexprn.Right.Type == leftVn.Type)
                            right = rV.Name;
                        else if (bexprn.Right is LiteralNode rLn && bexprn.Right.Type == leftVn.Type)
                            right = rLn.Value.ToString();
                        return $"{left} {bexprn.Operator} {right}";
                    }
                    if (bexprn.Right is BinaryExpressionNode rExpr)
                    {
                        string right = GenerateExpression(rExpr);
                        if (bexprn.Left is BinaryExpressionNode bexprL)
                        {
                            if (bexprL.Type == rExpr.Type)
                                left = GenerateExpression(bexprL);
                            else
                            {
                                try
                                {
                                    throw new CompilerError($"Cannot implicitly convert type '{bexprn.Type}' to '{bexprL.Type}'");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error: {ex.Message}");
                                    Environment.Exit(1);
                                }
                            }
                        }
                        return $"{left} {bexprn.Operator} {right}";
                    }
                    if (bexprn.Left is BinaryExpressionNode lExpr)
                    {
                        string right = GenerateExpression(lExpr);
                        if (bexprn.Right is BinaryExpressionNode bexprR)
                        {
                            if (bexprR.Type == lExpr.Type)
                                right = GenerateExpression(bexprR);
                            else
                            {
                                try
                                {
                                    throw new CompilerError($"Cannot implicitly convert type '{bexprn.Type}' to '{bexprR.Type}'");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error: {ex.Message}");
                                    Environment.Exit(1);
                                }
                            }
                        }
                        return $"{left} {bexprn.Operator} {right}";
                    }
                    throw new CompilerError("Invalid expression type");
                default:
                    return "";
            }
        }

        private static string MapAssignmentTypes(AssignmentType type)
        {
            return type switch
            {
                AssignmentType.Equal => "=",
                AssignmentType.PlusEqual => "+=",
                AssignmentType.MinusEqual => "-=",
                AssignmentType.StarEqual => "*=",
                AssignmentType.SlashEqual => "/=",
                _ => throw new Exception("Invalid Operator")
            };
        }

        private static string GenerateIfPyCode(IfNode @if, int indent = 1)
        {
            string indentation = new(' ', indent * 4);
            var lines = new List<string>();
            lines.Add($"if {GenerateExpression(@if.Condition)}:");
            switch (@if.ThenStatements)
            {
                case BlockNode block:
                    foreach (var stmt in block.Statements)
                    {
                        switch (stmt)
                        {
                            case DeclarationNode declaration:
                                lines.Add($"{indentation}{declaration.Name} = {GenerateExpression(declaration.Expression)}");
                                break;
                            case AssignementNode assignement:
                                switch (assignement.AssignmentType)
                                {
                                    case AssignmentType.PlusPlus:
                                        lines.Add($"{indentation}{assignement.Name} += 1");
                                        break;
                                    case AssignmentType.MinusMinus:
                                        lines.Add($"{indentation}{assignement.Name} -= 1");
                                        break;
                                    default:
                                        lines.Add($"{indentation}{assignement.Name} {MapAssignmentTypes(assignement.AssignmentType)} {GenerateExpression(assignement.Expression)}");
                                        break;
                                }
                                break;
                            case IfNode ifNodeInternal:
                                lines.Add($"{indentation}{GenerateIfPyCode(ifNodeInternal, indent + 1)}");
                                break;
                            case WhileNode @while:
                                lines.Add($"{indentation}{GenerateWhilePyCode(@while, indent + 1)}");
                                break;
                            case ConsoleNode console:
                                switch (console)
                                {
                                    case ConsoleOutputNode output:
                                        lines.Add($"{indentation}print({GenerateExpression(output.Object)})");
                                        break;
                                    case ConsoleInputNode input:
                                        lines.Add($"{indentation}input()");
                                        break;
                                }
                                break;
                        }
                    }
                    break;
                case StatementNode statement:
                    switch (statement)
                    {
                        case DeclarationNode:
                            throw new Exception("Embedded statement cannot be a declaration or labeled statement");
                        case AssignementNode assignement:
                            switch (assignement.AssignmentType)
                            {
                                case AssignmentType.PlusPlus:
                                    lines.Add($"{indentation}{assignement.Name} += 1");
                                    break;
                                case AssignmentType.MinusMinus:
                                    lines.Add($"{indentation}{assignement.Name} -= 1");
                                    break;
                                default:
                                    lines.Add($"{indentation}{assignement.Name} {MapAssignmentTypes(assignement.AssignmentType)} {GenerateExpression(assignement.Expression)}");
                                    break;
                            }
                            break;
                        case IfNode ifNodeInternal:
                            lines.Add($"{indentation}{GenerateIfPyCode(ifNodeInternal, indent + 1)}");
                            break;
                        case WhileNode @while:
                            lines.Add($"{indentation}{GenerateWhilePyCode(@while, indent + 1)}");
                            break;
                        case ConsoleNode console:
                            switch (console)
                            {
                                case ConsoleOutputNode output:
                                    lines.Add($"{indentation}print({GenerateExpression(output.Object)})");
                                    break;
                                case ConsoleInputNode input:
                                    lines.Add($"{indentation}input()");
                                    break;
                            }
                            break;
                    }
                    break;
            }
            foreach (var elif in @if.ElseIfStatements)
                lines.Add(GenerateElifPyCode(elif));
            if (@if.ElseStatements != null)
            {
                lines.Add($"else:");
                switch (@if.ElseStatements)
                {
                    case BlockNode block:
                        foreach (var stmt in block.Statements)
                        {
                            switch (stmt)
                            {
                                case DeclarationNode declaration:
                                    lines.Add($"{indentation}{declaration.Name} = {GenerateExpression(declaration.Expression)}");
                                    break;
                                case AssignementNode assignement:
                                    lines.Add($"{indentation}{assignement.Name} = {GenerateExpression(assignement.Expression)}");
                                    break;
                                case IfNode ifNodeInternal:
                                    lines.Add($"{indentation}{GenerateIfPyCode(ifNodeInternal, indent + 1)}");
                                    break;
                            }
                        }
                        break;
                    case StatementNode statement:
                        switch (statement)
                        {
                            case DeclarationNode:
                                throw new Exception("Embedded statement cannot be a declaration or labeled statement");
                            case AssignementNode assignement:
                                lines.Add($"{indentation}{assignement.Name} = {GenerateExpression(assignement.Expression)}");
                                break;
                            case IfNode ifNodeInternal:
                                lines.Add($"{indentation}{GenerateIfPyCode(ifNodeInternal, indent + 1)}");
                                break;
                        }
                        break;
                }
                if (lines.Count == 2)
                    lines = [];
            }
            if (lines.Count == 1)
                lines.Add("pass");
            return string.Join('\n', lines);
        }

        private static string GenerateElifPyCode(ElseIfNode elifNode, int indent = 1)
        {
            string indentation = new(' ', indent * 4);
            var lines = new List<string>();
            lines.Add($"elif {GenerateExpression(elifNode.Condition)}:");
            switch (elifNode.ThenStatements)
            {
                case BlockNode block:
                    foreach (var stmt in block.Statements)
                    {
                        switch (stmt)
                        {
                            case DeclarationNode declaration:
                                lines.Add($"{indentation}{declaration.Name} = {GenerateExpression(declaration.Expression)}");
                                break;
                            case AssignementNode assignement:
                                switch (assignement.AssignmentType)
                                {
                                    case AssignmentType.PlusPlus:
                                        lines.Add($"{assignement.Name} += 1");
                                        break;
                                    case AssignmentType.MinusMinus:
                                        lines.Add($"{assignement.Name} -= 1");
                                        break;
                                    default:
                                        lines.Add($"{assignement.Name} {MapAssignmentTypes(assignement.AssignmentType)} {GenerateExpression(assignement.Expression)}");
                                        break;
                                }
                                break;
                            case IfNode ifNodeInternal:
                                lines.Add($"{indentation}{GenerateIfPyCode(ifNodeInternal, indent + 1)}");
                                break;
                            case ElseIfNode elifInternal:
                                lines.Add($"{indentation}{GenerateElifPyCode(elifInternal, indent + 1)}");
                                break;
                            case ConsoleNode console:
                                switch (console)
                                {
                                    case ConsoleOutputNode output:
                                        lines.Add($"{indentation}print({GenerateExpression(output.Object)})");
                                        break;
                                    case ConsoleInputNode input:
                                        lines.Add($"{indentation}input()");
                                        break;
                                }
                                break;
                        }
                    }
                    break;
                case StatementNode statement:
                    switch (statement)
                    {
                        case DeclarationNode:
                            throw new Exception("Embedded statement cannot be a declaration or labeled statement");
                        case AssignementNode assignement:
                            switch (assignement.AssignmentType)
                            {
                                case AssignmentType.PlusPlus:
                                    lines.Add($"{assignement.Name} += 1");
                                    break;
                                case AssignmentType.MinusMinus:
                                    lines.Add($"{assignement.Name} -= 1");
                                    break;
                                default:
                                    lines.Add($"{assignement.Name} {MapAssignmentTypes(assignement.AssignmentType)} {GenerateExpression(assignement.Expression)}");
                                    break;
                            }
                            break;
                        case IfNode ifNodeInternal:
                            lines.Add($"{indentation}{GenerateIfPyCode(ifNodeInternal, indent + 1)}");
                            break;
                        case ElseIfNode elifInternal:
                            lines.Add($"{indentation}{GenerateElifPyCode(elifInternal, indent + 1)}");
                            break;
                        case ConsoleNode console:
                            switch (console)
                            {
                                case ConsoleOutputNode output:
                                    lines.Add($"{indentation}print({GenerateExpression(output.Object)})");
                                    break;
                                case ConsoleInputNode input:
                                    lines.Add($"{indentation}input()");
                                    break;
                            }
                            break;
                    }
                    break;
            }
            if (lines.Count == 1)
                lines = [];

            return string.Join('\n', lines);
        }

        private static string GenerateWhilePyCode(WhileNode @while, int indent = 1)
        {
            string indentation = new(' ', indent * 4);
            var lines = new List<string>();
            lines.Add($"while {GenerateExpression(@while.Condition)}:");
            switch (@while.Body)
            {
                case BlockNode block:
                    foreach (var stmt in block.Statements)
                    {
                        switch (stmt)
                        {
                            case DeclarationNode declaration:
                                lines.Add($"{indentation}{declaration.Name} = {GenerateExpression(declaration.Expression)}");
                                break;
                            case AssignementNode assignement:
                                switch (assignement.AssignmentType)
                                {
                                    case AssignmentType.PlusPlus:
                                        lines.Add($"{indentation}{assignement.Name} += 1");
                                        break;
                                    case AssignmentType.MinusMinus:
                                        lines.Add($"{indentation}{assignement.Name} -= 1");
                                        break;
                                    default:
                                        lines.Add($"{indentation}{assignement.Name} {MapAssignmentTypes(assignement.AssignmentType)} {GenerateExpression(assignement.Expression)}");
                                        break;
                                }
                                break;
                            case WhileNode whileInternal:
                                lines.Add($"{indentation}{GenerateWhilePyCode(whileInternal, indent + 1)}");
                                break;
                            case IfNode ifNodeInternal:
                                lines.Add($"{indentation}{GenerateIfPyCode(ifNodeInternal, indent + 1)}");
                                break;
                            case ElseIfNode elifInternal:
                                lines.Add($"{indentation}{GenerateElifPyCode(elifInternal, indent + 1)}");
                                break;
                            case ConsoleNode console:
                                switch (console)
                                {
                                    case ConsoleOutputNode output:
                                        lines.Add($"{indentation}print({GenerateExpression(output.Object)})");
                                        break;
                                    case ConsoleInputNode input:
                                        lines.Add($"{indentation}input()");
                                        break;
                                }
                                break;
                        }
                    }
                    break;
                case StatementNode statement:
                    switch (statement)
                    {
                        case DeclarationNode:
                            throw new Exception("Embedded statement cannot be a declaration or labeled statement");
                        case AssignementNode assignement:
                            switch (assignement.AssignmentType)
                            {
                                case AssignmentType.PlusPlus:
                                    lines.Add($"{indentation}{assignement.Name} += 1");
                                    break;
                                case AssignmentType.MinusMinus:
                                    lines.Add($"{indentation}{assignement.Name} -= 1");
                                    break;
                                default:
                                    lines.Add($"{indentation}{assignement.Name} {MapAssignmentTypes(assignement.AssignmentType)} {GenerateExpression(assignement.Expression)}");
                                    break;
                            }
                            break;
                        case WhileNode whileInternal:
                            lines.Add($"{indentation}{GenerateWhilePyCode(whileInternal, indent + 1)}");
                            break;
                        case IfNode ifNodeInternal:
                            lines.Add($"{indentation}{GenerateIfPyCode(ifNodeInternal, indent + 1)}");
                            break;
                        case ElseIfNode elifInternal:
                            lines.Add($"{indentation}{GenerateElifPyCode(elifInternal, indent + 1)}");
                            break;
                        case ConsoleNode console:
                            switch (console)
                            {
                                case ConsoleOutputNode output:
                                    lines.Add($"{indentation}print({GenerateExpression(output.Object)})");
                                    break;
                                case ConsoleInputNode input:
                                    lines.Add($"{indentation}input()");
                                    break;
                            }
                            break;
                    }
                    break;
            }
            if (lines.Count == 1)
                lines = [];
            return string.Join('\n', lines);
        }

        private static string GenerateForPyCode(ForNode @for, int indent = 1)
        {
            string indentation = new(' ', indent * 4);
            var lines = new List<string>();
            lines.Add($"{@for.ForExpression.StartPoint.Name} = {GenerateExpression(@for.ForExpression.StartPoint.Expression)}");
            lines.Add($"while {GenerateExpression(@for.ForExpression.Condition)}:");
            switch (@for.Body)
            {
                case BlockNode block:
                    foreach (var stmt in block.Statements)
                    {
                        switch (stmt)
                        {
                            case DeclarationNode declaration:
                                lines.Add($"{indentation}{declaration.Name} = {GenerateExpression(declaration.Expression)}");
                                break;
                            case AssignementNode assignement:
                                switch (assignement.AssignmentType)
                                {
                                    case AssignmentType.PlusPlus:
                                        lines.Add($"{indentation}{assignement.Name} += 1");
                                        break;
                                    case AssignmentType.MinusMinus:
                                        lines.Add($"{indentation}{assignement.Name} -= 1");
                                        break;
                                    default:
                                        lines.Add($"{indentation}{assignement.Name} {MapAssignmentTypes(assignement.AssignmentType)} {GenerateExpression(assignement.Expression)}");
                                        break;
                                }
                                break;
                            case WhileNode whileInternal:
                                lines.Add($"{indentation}{GenerateWhilePyCode(whileInternal, indent + 1)}");
                                break;
                            case IfNode ifNodeInternal:
                                lines.Add($"{indentation}{GenerateIfPyCode(ifNodeInternal, indent + 1)}");
                                break;
                            case ElseIfNode elifInternal:
                                lines.Add($"{indentation}{GenerateElifPyCode(elifInternal, indent + 1)}");
                                break;
                            case ForNode forInternal:
                                lines.Add($"{indentation}{GenerateForPyCode(forInternal, indent + 1)}");
                                break;
                            case ConsoleNode console:
                                switch (console)
                                {
                                    case ConsoleOutputNode output:
                                        lines.Add($"{indentation}print({GenerateExpression(output.Object)})");
                                        break;
                                    case ConsoleInputNode input:
                                        lines.Add($"{indentation}input()");
                                        break;
                                }
                                break;
                        }
                    }
                    break;
                case StatementNode statement:
                    switch (statement)
                    {
                        case DeclarationNode:
                            throw new Exception("Embedded statement cannot be a declaration or labeled statement");
                        case AssignementNode assignement:
                            switch (assignement.AssignmentType)
                            {
                                case AssignmentType.PlusPlus:
                                    lines.Add($"{indentation}{assignement.Name} += 1");
                                    break;
                                case AssignmentType.MinusMinus:
                                    lines.Add($"{indentation}{assignement.Name} -= 1");
                                    break;
                                default:
                                    lines.Add($"{indentation}{assignement.Name} {MapAssignmentTypes(assignement.AssignmentType)} {GenerateExpression(assignement.Expression)}");
                                    break;
                            }
                            break;
                        case WhileNode whileInternal:
                            lines.Add($"{indentation}{GenerateWhilePyCode(whileInternal, indent + 1)}");
                            break;
                        case IfNode ifNodeInternal:
                            lines.Add($"{indentation}{GenerateIfPyCode(ifNodeInternal, indent + 1)}");
                            break;
                        case ElseIfNode elifInternal:
                            lines.Add($"{indentation}{GenerateElifPyCode(elifInternal, indent + 1)}");
                            break;
                        case ForNode forInternal:
                            lines.Add($"{indentation}{GenerateForPyCode(forInternal, indent + 1)}");
                            break;
                        case ConsoleNode console:
                            switch (console)
                            {
                                case ConsoleOutputNode output:
                                    lines.Add($"{indentation}print({GenerateExpression(output.Object)})");
                                    break;
                                case ConsoleInputNode input:
                                    lines.Add($"{indentation}input()");
                                    break;
                            }
                            break;
                    }
                    break;
            }
            if (lines.Count == 1)
                lines = [];
            var steps = @for.ForExpression.Steps;
            if (steps != null)
            {
                switch (steps.AssignmentType)
                {
                    case AssignmentType.PlusPlus:
                        lines.Add($"{indentation}{steps.Name} += 1");
                        break;
                    case AssignmentType.MinusMinus:
                        lines.Add($"{indentation}{steps.Name} -= 1");
                        break;
                    default:
                        lines.Add($"{indentation}{steps.Name} {MapAssignmentTypes(steps.AssignmentType)} {GenerateExpression(steps.Expression)}");
                        break;
                }
            }
            return string.Join('\n', lines);
        }

        private static string GenerateMethodDeclarationPyCode(MethodDeclarationNode methodDeclaration, int indent = 1)
        {
            string indentation = new(' ', indent * 4);
            var lines = new List<string>();
            string parameters = "";
            for (int i = 0; i < methodDeclaration.Parameters.Length; i++)
            {
                if (i < methodDeclaration.Parameters.Length - 1)
                    parameters += $"{methodDeclaration.Parameters[i].Name}, ";
                else
                    parameters += $"{methodDeclaration.Parameters[i].Name}";
            }
            lines.Add($"def {methodDeclaration.Name}({parameters}):");

            string code = GeneratePyCode(methodDeclaration.Body, 1, 2);
            if (!string.IsNullOrEmpty(code))
                lines.Add(code);

            if (methodDeclaration.ReturnType == DataType.Void)
                lines.Add($"{indentation}return");
            else
            {
                foreach (var stmt in methodDeclaration.Body.Statements)
                {
                    if (stmt is ReturnNode @return)
                        lines.Add($"{indentation}return {GenerateExpression(@return.Value)}");
                }
            }

            if (lines.Count == 1)
                lines.Add($"{indentation}return");

            return string.Join('\n', lines);
        }
    }
}
