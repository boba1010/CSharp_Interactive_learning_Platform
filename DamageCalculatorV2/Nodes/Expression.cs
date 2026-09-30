using DamageCalculatorV2.Enums;

namespace DamageCalculatorV2.Nodes;

public sealed record Expression(object? Left, object? Right, SyntaxKind Operator)
{
    public override string ToString()
    {
        string left = "";
        string right = "";

        if (Left is Expression leftExpression)
            left = leftExpression.ToString();

        if (Right is Expression rightExpression)
            right = rightExpression.ToString();

        if (string.IsNullOrEmpty(left))
            left = $"{Left}";

        if (string.IsNullOrEmpty(right))
            right = $"{Right}";

        if (Right != null)
            return $"{left} {MapOperatorToString()} {right}";

        return $"{left}";
    }

    private string? MapOperatorToString()
    {
        return Operator switch
        {
            SyntaxKind.Addition => "+",
            SyntaxKind.Subtraction => "-",
            SyntaxKind.Multiplication => "*",
            SyntaxKind.Division => "/",
            SyntaxKind.Modulo => "%",
            _ => null,
        };
    }
}