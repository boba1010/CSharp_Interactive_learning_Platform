using CSharp_Interactive_Learning_App.Shared.Enums;

namespace CSharp_Interactive_Learning_App.Shared.DTOs;

public sealed record ExpressionDTO(object? Left, object? Right, SyntaxKind Operator);
