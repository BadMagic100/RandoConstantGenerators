using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace RandoConstantGenerators
{
    internal class MarkerAttributeReceiver : ISyntaxContextReceiver
    {
        public List<(INamedTypeSymbol type, AttributeData attr)> Classes { get; } = new();
        private readonly string markerAttrFullName;

        public MarkerAttributeReceiver(string markerAttrFullName)
        {
            this.markerAttrFullName = markerAttrFullName;
        }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is not ClassDeclarationSyntax cd
                || cd.AttributeLists.Count == 0
                || !cd.ChildTokens().Any(t => t.IsKind(SyntaxKind.StaticKeyword))
                || !cd.ChildTokens().Any(t => t.IsKind(SyntaxKind.PartialKeyword)))
            {
                return;
            }

            INamedTypeSymbol? ts = context.SemanticModel.GetDeclaredSymbol(cd);
            AttributeData? attr = ts?.GetAttributes()
                .FirstOrDefault(ad => markerAttrFullName == ad.AttributeClass?.ToDisplayString());
            if (ts != null && attr != null)
            {
                Classes.Add((ts, attr));
            }
        }
    }
}
