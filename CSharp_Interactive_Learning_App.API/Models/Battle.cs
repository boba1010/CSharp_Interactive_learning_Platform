using CSharp_Interactive_Learning_App.Shared.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSharp_Interactive_Learning_App.API.Models;

// must be synced with SyntaxKind
public enum OperationType
{
    Addition,
    Subtraction,
    Multiplication,
    Division,
    Concatenation,

    Increment,
    Decrement,

    Assignment,
    AdditionAssignment,
    SubtractionAssignment,
    MultiplicationAssignment,
    DivisionAssignment
}

public class Battle
{
    public int Id { get; set; }
    public int ChapterId { get; set; }

    public string Name { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Instructions { get; set; } = null!;
    public int EnemiesNumber { get; set; }
    public int HealthPerEnemy { get; set; }
    public double DmgMultiplier { get; set; }
    public int Points { get; set; }
    public bool IsSkippable { get; set; }

    [NotMapped]
    public HashSet<OperationType> AllowedOperationsSet => [.. AllowedOperations];
    [NotMapped]
    public HashSet<DataType> AllowedTypesSet => [.. AllowedTypes];

    public List<DataType> AllowedTypes { get; set; } = [];
    public List<BattleRequiredStatement> RequiredStatements { get; set; } = null!;
    public List<OperationType> AllowedOperations { get; set; } = [];
}
