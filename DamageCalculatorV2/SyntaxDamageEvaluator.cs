using DamageCalculatorV2.Enums;
using DamageCalculatorV2.Nodes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxKind = DamageCalculatorV2.Enums.SyntaxKind;

namespace DamageCalculatorV2;

public sealed class SyntaxDamageEvaluator(double multiplier) : CSharpSyntaxWalker
{
    private const int VariableDeclarationDamage = 5;
    private const int VariableAssignmentDamage = 2;
    private const int MathExpressionDamage = 3;

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
                Statements.Add(new VariableAssignmentStatement(variable.Name, MapExpression(node.Right)!));
                DamageDealt += VariableAssignmentDamage * multiplier;
            }

            if (IsMathExpression(node.Right))
                DamageDealt += MathExpressionDamage;
        }

        base.VisitAssignmentExpression(node);
    }

    private static Expression? MapExpression(ExpressionSyntax? expression)
    {
        if (expression is null)
            return null;

        if (expression is BinaryExpressionSyntax binary)
            return new Expression(binary.Left.ToString(), binary.Right.ToString(), MapBinaryOperator(binary.Kind()));

        return null;
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

    private static SyntaxKind MapBinaryOperator(Microsoft.CodeAnalysis.CSharp.SyntaxKind kind)
    {
        return kind switch
        {
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.AddExpression => SyntaxKind.Addition,
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.SubtractExpression => SyntaxKind.Subtraction,
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.MultiplyExpression => SyntaxKind.Multiplication,
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.DivideExpression => SyntaxKind.Division,
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
