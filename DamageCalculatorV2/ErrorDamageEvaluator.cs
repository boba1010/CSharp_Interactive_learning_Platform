using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DamageCalculatorV2;

public sealed class ErrorDamageEvaluator(double multiplier, CSharpCompilation compilation)
{
    private const int CS1002SemicolonMissingError = 2;

    public double DamageTaken { get; private set; }

    public List<string> Evaluate()
    {
        List<string> errors = [];

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
