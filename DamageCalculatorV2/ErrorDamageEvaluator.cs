using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DamageCalculatorV2;

public sealed class ErrorDamageEvaluator(double multiplier, CSharpCompilation compilation)
{
    private const int CS1002SemicolonMissingError = 2;
    private const int CS1003SyntaxError = 2;
    private const int CS0103UnknownNameError = 3;
    private const int CS0131InvalidAssignmentTargetError = 3;
    private const int CS0019InvalidOperatorError = 3;
    private const int CS0029CannotConvertTypeError = 3;
    private const int CS0266ExplicitConversionRequiredError = 3;
    private const int CS0023InvalidUnaryOperatorError = 3;
    private const int CS0165UnassignedVariableError = 3;
    private const int CS1513ClosingBraceExpectedError = 2;
    private const int CS1026ClosingParenthesisExpectedError = 2;
    private const int CS1525InvalidExpressionTermError = 2;

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
        DamageTaken += diagnostic.Id switch
        {
            "CS1002" => CS1002SemicolonMissingError,
            "CS1003" => CS1003SyntaxError,
            "CS0103" => CS0103UnknownNameError,
            "CS0131" => CS0131InvalidAssignmentTargetError,
            "CS0019" => CS0019InvalidOperatorError,
            "CS0029" => CS0029CannotConvertTypeError,
            "CS0266" => CS0266ExplicitConversionRequiredError,
            "CS0023" => CS0023InvalidUnaryOperatorError,
            "CS0165" => CS0165UnassignedVariableError,
            "CS1513" => CS1513ClosingBraceExpectedError,
            "CS1026" => CS1026ClosingParenthesisExpectedError,
            "CS1525" => CS1525InvalidExpressionTermError,
            _ => 0
        } * multiplier;
    }

    private static string GetFriendlyError(Diagnostic diagnostic)
    {
        var startLinePos = diagnostic.Location.GetLineSpan().StartLinePosition;

        var line = startLinePos.Line + 1;
        var column = startLinePos.Character + 1;

        return diagnostic.Id switch
        {
            "CS1002" =>
                $"{diagnostic.Id}: You forgot to end the statement with a semicolon at line {line}, column {column}",

            "CS1003" =>
                $"{diagnostic.Id}: There is a syntax error at line {line}, column {column}",

            "CS0103" =>
                $"{diagnostic.Id}: The variable '{GetName(diagnostic)}' does not exist at line {line}, column {column}",

            "CS0131" =>
                $"{diagnostic.Id}: You cannot assign a value to this expression at line {line}, column {column}",

            "CS0019" =>
                $"{diagnostic.Id}: This operator cannot be used with these types at line {line}, column {column}",

            "CS0029" =>
                $"{diagnostic.Id}: You cannot convert this value to the required type at line {line}, column {column}",

            "CS0266" =>
                $"{diagnostic.Id}: You need an explicit conversion between these types at line {line}, column {column}",

            "CS0023" =>
                $"{diagnostic.Id}: This operator cannot be used with this type at line {line}, column {column}",

            "CS0165" =>
                $"{diagnostic.Id}: You are using a variable before giving it a value at line {line}, column {column}",

            "CS1513" =>
                $"{diagnostic.Id}: You forgot to close a block with '}}' at line {line}, column {column}",

            "CS1026" =>
                $"{diagnostic.Id}: You forgot to close a parenthesis at line {line}, column {column}",

            "CS1525" =>
                $"{diagnostic.Id}: This is not a valid expression at line {line}, column {column}",

            _ =>
                $"{diagnostic.Id}: {diagnostic.GetMessage()} at line {line}, column {column}"
        };
    }

    private static string GetName(Diagnostic diagnostic)
    {
        var message = diagnostic.GetMessage();
        var start = message.IndexOf('\'');

        if (start < 0)
            return "unknown";

        var end = message.IndexOf('\'', start + 1);

        return end < 0 ? "unknown" : message[(start + 1)..end];
    }
}