using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace RandoConstantGenerators
{
    internal class MarkerAttributeReceiver : ISyntaxContextReceiver
    {
        private static readonly DiagnosticDescriptor NotPartialStatic = new(
            id: "RCG001",
            title: "Marked class is not static and partial",
            messageFormat: "The class '{0}' marked with '{1}' must be static and partial for generation to occur",
            category: "RandoConstantGenerators",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public List<(INamedTypeSymbol type, AttributeData attr)> Classes { get; } = new();
        public List<Diagnostic> PreprocessorDiagnostics = new();
        private readonly string markerAttrFullName;

        public MarkerAttributeReceiver(string markerAttrFullName)
        {
            this.markerAttrFullName = markerAttrFullName;
        }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is not ClassDeclarationSyntax cd
                || cd.AttributeLists.Count == 0)
            {
                return;
            }


            INamedTypeSymbol attrSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(markerAttrFullName)!;

            INamedTypeSymbol? ts = context.SemanticModel.GetDeclaredSymbol(cd);
            AttributeData? attr = ts?.GetAttributes()
                .FirstOrDefault(ad => attrSymbol.Equals(ad.AttributeClass, SymbolEqualityComparer.Default));
            if (ts == null || attr == null)
            {
                return;
            }

            if (!cd.ChildTokens().Any(t => t.IsKind(SyntaxKind.StaticKeyword)) 
                || !cd.ChildTokens().Any(t => t.IsKind(SyntaxKind.PartialKeyword))) 
            {
                SyntaxToken ident = cd.ChildTokens().First(t => t.IsKind(SyntaxKind.IdentifierToken));
                PreprocessorDiagnostics.Add(Diagnostic.Create(NotPartialStatic, Location.Create(cd.SyntaxTree, ident.Span),
                    ts.ToDisplayString(), attrSymbol.Name));
                return;
            }

            Classes.Add((ts, attr));
        }
    }
}
