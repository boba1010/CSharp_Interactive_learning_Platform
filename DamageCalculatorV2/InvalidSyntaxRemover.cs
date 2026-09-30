using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DamageCalculatorV2;

public class InvalidSyntaxRemover
{
    public static SyntaxTree Filter(SyntaxTree tree)
    {
        var root = tree.GetRoot();

        var diagnostics = tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error && d.Location.IsInSource).ToArray();

        var members = root.ChildNodes().Where(member => !HasError(member, diagnostics)).ToArray();

        var cleanRoot = ((CompilationUnitSyntax)root).WithMembers(SyntaxFactory.List(members.Cast<MemberDeclarationSyntax>()));

        return CSharpSyntaxTree.Create(cleanRoot);
    }

    private static bool HasError(SyntaxNode node, Diagnostic[] diagnostics)
    {
        return diagnostics.Any(d => d.Location.SourceSpan.IntersectsWith(node.FullSpan));
    }
}
