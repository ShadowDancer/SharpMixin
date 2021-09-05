using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpMixin.Attributes;
using SharpMixin.Generator.Configuration;

namespace SharpMixin.Generator.Generator
{
    /// <summary>
    /// Discovers mixin classes and passes them to <see cref="ClassGenerator"/>
    /// </summary>
    public class GenerationOrchestrator
    {
        private readonly GeneratorExecutionContext _context;

        public GenerationOrchestrator(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public void Generate()
        {
            var compilation = _context.Compilation;

            IEnumerable<SyntaxNode> allNodes =
                compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
            IEnumerable<TypeDeclarationSyntax> allClasses = allNodes
                .Where(d => d.IsKind(SyntaxKind.ClassDeclaration) || d.IsKind(SyntaxKind.RecordDeclaration))
                .OfType<TypeDeclarationSyntax>();

            var generatedSource = allClasses
                .Select(classDeclaration => TryGenerateMixin(compilation, classDeclaration))
                .Where(n => n != null)
                .Select(n => n!.Value)
                .ToImmutableArray();

            foreach ((string className, string source) in generatedSource) _context.AddSource(className, source);
        }

        private (string className, string source)? TryGenerateMixin(Compilation compilation,
            TypeDeclarationSyntax classDeclaration)
        {
            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            if (!IsMixin(classDeclaration, semanticModel))
            {
                return null;
            }

            var config = ConfigurationCollector.Collect(_context, semanticModel, classDeclaration);
            var source = ClassGenerator.GenerateSource(classDeclaration, semanticModel, config);
            
            if (semanticModel.GetDeclaredSymbol(classDeclaration) is not ITypeSymbol type)
            {
                return (Guid.NewGuid().ToString(), source);
            }

            string fullName = type.ContainingNamespace.ToDisplayString() + "." + type.Name;
            return (fullName, source);
        }

        private bool IsMixin(TypeDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            var mixinAttribute = semanticModel.Compilation.GetTypeByMetadataName(typeof(MixinAttribute).FullName);
            var hasMixinAttribute = classDeclaration.AttributeLists
                .SelectMany(n => n.Attributes).Any(attribute =>
                {
                    if (semanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeConstructor)
                    {
                        return false;
                    }

                    var attributeSymbol = attributeConstructor.ContainingType;
                    return SymbolEqualityComparer.Default.Equals(attributeSymbol, mixinAttribute);
                });
            if (!hasMixinAttribute)
            {
                return false;
            }
            
            var isPartial = classDeclaration.Modifiers.Any(n => n.IsKeyword() && n.ToString() == "partial");
            if (!isPartial)
            {
                var nonPartialDiagnostic =
                    Diagnostic.Create(Diagnostics.NonPartialMixin, classDeclaration.GetLocation(), classDeclaration.Identifier.Text);
                _context.ReportDiagnostic(nonPartialDiagnostic);
                return false;
            }
            
            var containingClasses = GetContainingTypes(classDeclaration);
            bool anyParentIsNotPartial = false;
            foreach (var typeSymbol in containingClasses)
            {
                bool isParentPartial = typeSymbol.Modifiers.Any(n => n.ToString() == "partial");
                if (!isParentPartial)
                {
                    var nonPartialDiagnostic =
                        Diagnostic.Create(Diagnostics.MixinInNonPartialClass, typeSymbol.GetLocation(), 
                            classDeclaration.Identifier.Text, typeSymbol.Identifier.Text);
                    _context.ReportDiagnostic(nonPartialDiagnostic);
                    anyParentIsNotPartial = true;
                }
            }

            if (anyParentIsNotPartial)
            {
                return false;
            }

            INamedTypeSymbol classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration)!;
            if (classSymbol.Interfaces.Length == 0)
            {
                var noInterfacesDiagnostic = Diagnostic.Create(Diagnostics.MixinWithoutInterfaces, 
                    classDeclaration.GetLocation(), classDeclaration.Identifier.Text);
                _context.ReportDiagnostic(noInterfacesDiagnostic);
            }
            
            return true;
        }

        private IReadOnlyCollection<TypeDeclarationSyntax> GetContainingTypes(TypeDeclarationSyntax classDeclaration)
        {
            SyntaxNode? current = classDeclaration;
            List<TypeDeclarationSyntax> parentTypes = new();
            
            while (true)
            {
                current = current.Parent;
                if (current == null)
                {
                    break;
                }

                if (current is TypeDeclarationSyntax typeSyntax)
                {
                    parentTypes.Add(typeSyntax);
                }
            }

            return parentTypes;
        }
    }
}