using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using SharpMixin.Attributes;

namespace SharpMixin.Generator.Configuration
{
    public class ConfigurationCollector
    {
        private readonly GeneratorExecutionContext _context;
        private readonly SemanticModel _semanticModel;
        private readonly TypeDeclarationSyntax _mixinDeclaration;
        private readonly INamedTypeSymbol? _constructUsingAttributeSymbol;

        private ConfigurationCollector(GeneratorExecutionContext context, SemanticModel semanticModel,
            TypeDeclarationSyntax mixinDeclaration)
        {
            _context = context;
            _semanticModel = semanticModel;
            _mixinDeclaration = mixinDeclaration;
            _constructUsingAttributeSymbol =
                _context.Compilation.GetTypeByMetadataName(typeof(ConstructUsingAttribute).FullName);
        }

        public static MixinConfiguration Collect(GeneratorExecutionContext context, SemanticModel semanticModel,
            TypeDeclarationSyntax mixinDeclaration)
        {
            return new ConfigurationCollector(context, semanticModel, mixinDeclaration).Collect();
        }

        private MixinConfiguration Collect()
        {
            ImmutableArray<ConstructorConfiguration> attributes = _mixinDeclaration.AttributeLists
                .SelectMany(n => n.Attributes)
                .Select(CreateConstructorConfiguration)
                .Where(n => n != null)
                .ToImmutableArray()!;

            return new MixinConfiguration(true, attributes.ToImmutableArray());
        }

        private ConstructorConfiguration? CreateConstructorConfiguration(AttributeSyntax attribute)
        {
            var types = GetTypesFromConstructUsing(attribute);
            if (types.Count == 0)
            {
                return null;
            }

            return MapTypesToMixinInterfaces(attribute, types);

        }
        

        private IReadOnlyList<INamedTypeSymbol> GetTypesFromConstructUsing(AttributeSyntax attribute)
        {
            if (_semanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeConstructor)
            {
                return Array.Empty<INamedTypeSymbol>();
            }

            var attributeSymbol = attributeConstructor.ContainingType;
            if (!SymbolEqualityComparer.Default.Equals(attributeSymbol, _constructUsingAttributeSymbol))
            {
                return Array.Empty<INamedTypeSymbol>();
            }

            bool hasNoArguments = attribute.ArgumentList == null ||
                                  attribute.ArgumentList.Arguments.Count < 1;
            if (hasNoArguments)
            {
                _context.ReportDiagnostic(Diagnostic.Create(Diagnostics.ConstructUsing.EmptyConstructor,
                    attribute.GetLocation()));
                return Array.Empty<INamedTypeSymbol>();
            }

            List<INamedTypeSymbol> foundTypes = new();
            bool failed = false;
            foreach (AttributeArgumentSyntax argument in attribute.ArgumentList!.Arguments)
            {
                if (argument.Expression is not TypeOfExpressionSyntax typeOfExpression)
                {
                    _context.ReportDiagnostic(Diagnostic.Create(Diagnostics.ConstructUsing.CannotParseType,
                        argument.GetLocation(), argument.ToString()));
                    failed = true;
                    continue;
                }

                ISymbol? typeSymbol = _semanticModel.GetSymbolInfo(typeOfExpression.Type).Symbol;
                if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
                {
                    _context.ReportDiagnostic(Diagnostic.Create(Diagnostics.ConstructUsing.CannotParseType,
                        argument.GetLocation(), argument.ToString()));
                    failed = true;
                    continue;
                }

                foundTypes.Add(namedTypeSymbol);
            }

            if (failed)
            {
                return Array.Empty<INamedTypeSymbol>();
            }

            return foundTypes;
        }

        private ConstructorConfiguration MapTypesToMixinInterfaces(AttributeSyntax attribute,
            IReadOnlyList<INamedTypeSymbol> constructUsingArguments)
        {
            var mixinDeclaration = (INamedTypeSymbol)_semanticModel.GetDeclaredSymbol(_mixinDeclaration)!;

            List<INamedTypeSymbol> interfacesToConsume = mixinDeclaration.Interfaces.ToList();
            List<INamedTypeSymbol> typesToConsume = constructUsingArguments.ToList();

            Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> interfaceMappings =
                constructUsingArguments.ToDictionary(n => n, n => new List<INamedTypeSymbol>());

            foreach (INamedTypeSymbol typeSymbol in typesToConsume.ToArray())
            {
                foreach (INamedTypeSymbol interfaceSymbol in interfacesToConsume.ToArray())
                {
                    CommonConversion conversion =
                        _context.Compilation.ClassifyCommonConversion(typeSymbol, interfaceSymbol);
                    if (conversion.IsIdentity)
                    {
                        typesToConsume.Remove(typeSymbol);
                        interfacesToConsume.Remove(interfaceSymbol);
                        interfaceMappings[typeSymbol].Add(interfaceSymbol);
                    }
                }
            }

            foreach (INamedTypeSymbol typeSymbol in typesToConsume.ToArray())
            {
                foreach (INamedTypeSymbol interfaceSymbol in interfacesToConsume.ToArray())
                {
                    CommonConversion conversion =
                        _context.Compilation.ClassifyCommonConversion(typeSymbol, interfaceSymbol);
                    if (conversion.IsImplicit)
                    {
                        typesToConsume.Remove(typeSymbol);
                        interfacesToConsume.Remove(interfaceSymbol);
                        interfaceMappings[typeSymbol].Add(interfaceSymbol);
                    }
                }
            }

            if (typesToConsume.Any())
            {
                foreach (INamedTypeSymbol typeSymbol in typesToConsume)
                {
                    _context.ReportDiagnostic(Diagnostic.Create(Diagnostics.ConstructUsing.UnusedInterface,
                        attribute.GetLocation(), typeSymbol.Name, _mixinDeclaration.Identifier.Text));
                }
            }

            return new ConstructorConfiguration(interfaceMappings
                .Select(n => new ConstructorTypeMapping(n.Key, n.Value.ToImmutableArray()))
                .ToImmutableArray());
        }
    }
}