namespace CSharp_Interactive_Learning_App.Shared.DTOs;

public sealed record VariableAssignmentDTO(string Name, ExpressionDTO Value) : StatementDTO;
