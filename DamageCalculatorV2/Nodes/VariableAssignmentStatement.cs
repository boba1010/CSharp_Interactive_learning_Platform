namespace DamageCalculatorV2.Nodes;

public sealed record VariableAssignmentStatement(string Name, Expression Value) : Statement;
