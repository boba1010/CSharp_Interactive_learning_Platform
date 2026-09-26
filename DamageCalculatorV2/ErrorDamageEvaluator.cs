using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace DamageCalculatorV2;

public sealed class ErrorDamageEvaluator(double multiplier)
{
    private const int CS1002SemicolonMissingError = 5;

    public double DamageTaken { get; private set; }

    public List<string> Evaluate(SyntaxTree tree)
    {
        List<MetadataReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
        ];

        List<string> errors = [];

        var compilation = CSharpCompilation.Create("Temp", [tree], references);

        foreach (var diagnostic in compilation.GetDiagnostics())
        {
            if (diagnostic.Severity != DiagnosticSeverity.Error)
                continue;

            CalculateDamage(diagnostic);
            errors.Add(GetFriendlyError(diagnostic));
        }

        return errors;
    }

    private void CalculateDamage(Diagnostic diagnostic)
    {
        switch (diagnostic.Id)
        {
            case "CS1002":
                DamageTaken += CS1002SemicolonMissingError * multiplier;
                break;
        }
    }

    private static string GetFriendlyError(Diagnostic diagnostic)
    {
        var startLinePos = diagnostic.Location.GetLineSpan().StartLinePosition;

        var line = startLinePos.Line + 1;
        var column = startLinePos.Character + 1;

        return diagnostic.Id switch
        {
            "CS1002" => $"{diagnostic.Id}: You forgot to end the statement with a semicolon at line {line}, column {column}",
            _ => null!
        };
    }
}
