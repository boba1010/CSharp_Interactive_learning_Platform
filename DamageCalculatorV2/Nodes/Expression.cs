using DamageCalculatorV2.Enums;

namespace DamageCalculatorV2.Nodes;

public sealed record Expression(object? Left, object? Right, SyntaxKind Operator);