using CSharp_Interactive_Learning_App.Shared.Enums;

namespace CSharp_Interactive_Learning_App.Shared.Models;

public sealed record Variable(DataType Type, string Name, object? Value);