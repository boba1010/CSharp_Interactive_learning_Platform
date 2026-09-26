using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DamageCalculatorV2;

public class InvalidSyntaxRemover
{
    public static CompilationUnitSyntax Filter(CompilationUnitSyntax root)
    {
        var diagnostics = root.SyntaxTree
            .GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error && d.Location.IsInSource)
            .ToArray();

        var members = root.Members.Where(member => !HasError(member, diagnostics)).ToArray();

        return root.WithMembers(SyntaxFactory.List(members));
    }

    private static bool HasError(SyntaxNode node, Diagnostic[] diagnostics)
    {
        return diagnostics.Any(d => d.Location.SourceSpan.IntersectsWith(node.FullSpan));
    }
}
