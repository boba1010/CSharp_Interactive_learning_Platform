using CSharp_Interactive_Learning_App.Shared.Enums;

namespace CSharp_Interactive_Learning_App.Shared.DTOs;

public sealed record VariableDeclarationDTO(DataType Type, string Name, ExpressionDTO? Value) : StatementDTO;
