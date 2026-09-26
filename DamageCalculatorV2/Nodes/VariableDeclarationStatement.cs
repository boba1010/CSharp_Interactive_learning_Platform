using DamageCalculatorV2.Enums;

namespace DamageCalculatorV2.Nodes;

public sealed record VariableDeclarationStatement(DataType Type, string Name, Expression? Value) : Statement;
