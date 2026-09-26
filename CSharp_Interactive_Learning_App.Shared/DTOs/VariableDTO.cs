using CSharp_Interactive_Learning_App.Shared.Enums;

namespace CSharp_Interactive_Learning_App.Shared.DTOs;

public sealed record VariableDTO(DataType Type, string Name, object? Value);
