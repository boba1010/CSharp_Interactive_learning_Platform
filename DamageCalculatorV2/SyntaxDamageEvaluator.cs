using DamageCalculatorV2.Enums;
using DamageCalculatorV2.Nodes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxKind = DamageCalculatorV2.Enums.SyntaxKind;

namespace DamageCalculatorV2;

public sealed class SyntaxDamageEvaluator(double multiplier, SemanticModel semanticModel) : CSharpSyntaxWalker
{
    private const int VariableDeclarationDamage = 5;
    private const int VariableAssignmentDamage = 2;
    private const int MathExpressionDamage = 3;
    private const int ConcatExpressionDamage = 3;

    public double DamageDealt { get; private set; }
    public List<Statement> Statements { get; } = [];

    public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
    {
        var type = node.Type;

        foreach (var variable in node.Variables)
        {
            var name = variable.Identifier.Text;
            var value = MapExpression(variable.Initializer?.Value);

            DamageDealt += VariableDeclarationDamage * multiplier;

            Statements.Add(new VariableDeclarationStatement(MapCSharpType(type), name, value));

            if (IsStringConcatenation(variable.Initializer?.Value, semanticModel))
                DamageDealt += ConcatExpressionDamage;

            if (IsMathExpression(variable.Initializer?.Value))
                DamageDealt += MathExpressionDamage;
        }

        base.VisitVariableDeclaration(node);
    }

    public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        if (node.Left is IdentifierNameSyntax identifier)
        {
            var index = Statements.FindIndex(x => x is VariableDeclarationStatement variable && 
                variable.Name == identifier.Identifier.Text);

            if (index >= 0 && Statements[index] is VariableDeclarationStatement variable)
            {
                Statements.Add(new VariableAssignmentStatement(variable.Name, MapExpression(node.Right)!, MapOperator(node.Kind())));
                DamageDealt += VariableAssignmentDamage * multiplier;
            }

            if (IsStringConcatenation(node.Right, semanticModel))
                DamageDealt += ConcatExpressionDamage;

            if (IsMathExpression(node.Right))
                DamageDealt += MathExpressionDamage;
        }

        base.VisitAssignmentExpression(node);
    }

    public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
    {
        if (node.Kind() is
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.PreIncrementExpression or
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.PreDecrementExpression)
        {
            HandleIncrementOrDecrement(node);
        }

        base.VisitPrefixUnaryExpression(node);
    }

    public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
    {
        if (node.Kind() is
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.PostIncrementExpression or
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.PostDecrementExpression)
        {
            HandleIncrementOrDecrement(node);
        }

        base.VisitPostfixUnaryExpression(node);
    }

    private static Expression? MapExpression(ExpressionSyntax? expression)
    {
        if (expression is null)
            return null;

        if (expression is BinaryExpressionSyntax binary)
            return new Expression(binary.Left.ToString(), binary.Right.ToString(), MapOperator(binary.Kind()));

        if (expression is AssignmentExpressionSyntax assignment)
            return new Expression(assignment.Left.ToString(), assignment.Right.ToString(), MapOperator(assignment.Kind()));

        if (expression is PrefixUnaryExpressionSyntax prefix)
            return new Expression(prefix.Operand.ToString(), null, MapOperator(prefix.Kind()));

        if (expression is PostfixUnaryExpressionSyntax postfix)
            return new Expression(postfix.Operand.ToString(), null, MapOperator(postfix.Kind()));

        return new(expression.ToString(), null, SyntaxKind.None);
    }

    private void HandleIncrementOrDecrement(ExpressionSyntax node)
    {
        var expression = MapExpression(node)!;

        if (node is not PrefixUnaryExpressionSyntax && node is not PostfixUnaryExpressionSyntax)
            return;

        var operand = node switch
        {
            PrefixUnaryExpressionSyntax prefix => prefix.Operand,
            PostfixUnaryExpressionSyntax postfix => postfix.Operand,
            _ => null
        };

        if (operand is not IdentifierNameSyntax identifier)
            return;

        var variable = Statements
            .OfType<VariableDeclarationStatement>()
            .FirstOrDefault(x => x.Name == identifier.Identifier.Text);

        if (variable is null)
            return;

        Statements.Add(new VariableAssignmentStatement(variable.Name, new(expression.Right, null, SyntaxKind.None), MapOperator(node.Kind())));
        DamageDealt += VariableAssignmentDamage * multiplier;
    }

    private static bool IsMathExpression(ExpressionSyntax? expression)
    {
        return expression?.Kind() is
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.AddExpression or
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.SubtractExpression or
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.MultiplyExpression or
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.DivideExpression or
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.ModuloExpression;
    }

    private static bool IsStringConcatenation(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        if (expression.Kind() != Microsoft.CodeAnalysis.CSharp.SyntaxKind.AddExpression)
            return false;

        if (expression is not BinaryExpressionSyntax binary)
            return false;

        var leftType = semanticModel.GetTypeInfo(binary.Left).Type;
        var rightType = semanticModel.GetTypeInfo(binary.Right).Type;

        return leftType?.SpecialType == SpecialType.System_String || rightType?.SpecialType == SpecialType.System_String;
    }

    private static SyntaxKind MapOperator(Microsoft.CodeAnalysis.CSharp.SyntaxKind kind)
    {
        return kind switch
        {
            // binary
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.AddExpression => SyntaxKind.Addition,
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.SubtractExpression => SyntaxKind.Subtraction,
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.MultiplyExpression => SyntaxKind.Multiplication,
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.DivideExpression => SyntaxKind.Division,
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.ModuloExpression => SyntaxKind.Modulo,

            // Assignment
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression => SyntaxKind.Assignment,
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.AddAssignmentExpression => SyntaxKind.AdditionAssignment,
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.SubtractAssignmentExpression => SyntaxKind.SubtractionAssignment,

            // Unary
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.PreIncrementExpression => SyntaxKind.PreIncrement,
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.PostIncrementExpression => SyntaxKind.PostIncrement,
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.PreDecrementExpression => SyntaxKind.PreDecrement,
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.PostDecrementExpression => SyntaxKind.PostDecrement,
            _ => throw new NotSupportedException($"Unsupported binary operator: {kind}")
        };
    }

    private static DataType MapCSharpType(TypeSyntax type)
    {
        return type.ToString() switch
        {
            "int" => DataType.Int,
            "double" => DataType.Double,
            "float" => DataType.Float,
            "long" => DataType.Long,
            "string" => DataType.String,
            "object" => DataType.Object,
            _ => DataType.Void,
        };
    }
}
