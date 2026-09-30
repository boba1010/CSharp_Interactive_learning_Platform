using DamageCalculatorV2.Enums;

namespace DamageCalculatorV2.Nodes;

public sealed record VariableAssignmentStatement(string Name, Expression Value, SyntaxKind Kind) : Statement
{
    public override string ToString()
    {
        if (Kind is SyntaxKind.PreIncrement or SyntaxKind.PreDecrement)
            return $"{MapOperatorToString()}{Name}";

        if (Kind is SyntaxKind.PostIncrement or SyntaxKind.PostDecrement)
            return $"{Name}{MapOperatorToString()}";

        return $"{Name} {MapOperatorToString()} {Value}";
    }

    private string? MapOperatorToString()
    {
        return Kind switch
        {
            SyntaxKind.Assignment => "=",
            SyntaxKind.AdditionAssignment => "+=",
            SyntaxKind.SubtractionAssignment => "-=",
            SyntaxKind.MultiplicationAssignment => "*=",
            SyntaxKind.DivisionAssignment => "/=",
            SyntaxKind.ModuloAssignment => "%=",
            SyntaxKind.PreDecrement => "--",
            SyntaxKind.PreIncrement => "++",
            SyntaxKind.PostDecrement => "--",
            SyntaxKind.PostIncrement => "++",
            _ => null,
        };
    }
}
