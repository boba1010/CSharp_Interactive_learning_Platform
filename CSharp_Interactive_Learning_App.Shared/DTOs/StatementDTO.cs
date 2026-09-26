using System.Text.Json.Serialization;

namespace CSharp_Interactive_Learning_App.Shared.DTOs;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(VariableDeclarationDTO), "variableDeclaration")]
[JsonDerivedType(typeof(VariableAssignmentDTO), "variableAssignment")]
public abstract record StatementDTO();
