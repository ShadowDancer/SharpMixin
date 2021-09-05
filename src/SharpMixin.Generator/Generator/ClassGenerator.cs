using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpMixin.Generator.Configuration;
using SharpMixin.Generator.Generator.Utils;

namespace SharpMixin.Generator.Generator
{
    /// <summary>
    /// Generates source for mixin class, delegates construction to other classes
    /// </summary>
    public class ClassGenerator
    {
        private readonly SemanticModel _semanticModel;
        private readonly MixinConfiguration _configuration;
        private readonly INamedTypeSymbol _typeSymbol;
        private readonly ImmutableArray<InterfaceData> _interfaces;

        public static string GenerateSource(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel, MixinConfiguration configuration)
        {
            return new ClassGenerator(typeDeclaration, semanticModel, configuration).GenerateSource(); 
        }
        
        private ClassGenerator(
            TypeDeclarationSyntax typeDeclaration,
            SemanticModel semanticModel,
            MixinConfiguration configuration)
        {
            _semanticModel = semanticModel;
            _configuration = configuration;
            _typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol
                          ?? throw new ArgumentException(nameof(typeDeclaration));

            _interfaces = _typeSymbol.Interfaces
                .Select(n => new InterfaceData(n))
                .ToImmutableArray();
        }

        private string GenerateSource()
        {
            // begin creating the source we'll inject into the users compilation
            using var writer = new StringWriter();
            var sourceWriter = new IndentedTextWriter(writer);


            GenerateNamespace(sourceWriter);
            using (sourceWriter.Scope())
            {
                List<IDisposable> containingTypeScopes = new();
                var containingTypes = _typeSymbol.GetContainingTypes();
                foreach (var containingType in containingTypes.Reverse())
                {
                    AddClassHeader(sourceWriter, containingType);
                    containingTypeScopes.Add(sourceWriter.Scope());
                }

                AddClassHeader(sourceWriter, _typeSymbol);

                using (sourceWriter.Scope())
                {
                    if (_configuration.GenerateDefaultConstructor)
                    {
                        var constructorParams = ConstructorParamConfigurator
                            .MapTypesToFields(_semanticModel.Compilation, _interfaces, ConstructorConfiguration.Empty());
                        ConstructorGenerator.GenerateConstructor(sourceWriter, constructorParams, _typeSymbol.Name);
                    }

                    foreach (var additionalConstructor in _configuration.AdditionalConstructors)
                    {
                        var constructorParams = ConstructorParamConfigurator
                            .MapTypesToFields(_semanticModel.Compilation, _interfaces, additionalConstructor);
                        ConstructorGenerator.GenerateConstructor(sourceWriter, constructorParams, _typeSymbol.Name);
                    }

                    GenerateFields(sourceWriter);

                    var interfaceInfo = 
                    _interfaces
                        .Select(n => new InterfaceProxyGenerator.InterfaceInfo(n.TypeSymbol, n.FieldName))
                        .ToImmutableArray();
                    InterfaceProxyGenerator.GenerateInterfaceProxies(sourceWriter, interfaceInfo);
                }

                foreach (var scope in containingTypeScopes)
                {
                    scope.Dispose();
                }
            }

            sourceWriter.Flush();
            return writer.GetStringBuilder().ToString();
        }

        private void GenerateNamespace(IndentedTextWriter sourceWriter)
        {
            string namespaceDeclaration = _typeSymbol.ContainingNamespace.ToDisplayString(
                new SymbolDisplayFormat(
                    SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
                    SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                    kindOptions: SymbolDisplayKindOptions.IncludeNamespaceKeyword));

            sourceWriter.WriteLine(namespaceDeclaration);
        }

        private void AddClassHeader(IndentedTextWriter sourceWriter, INamedTypeSymbol typeSymbol)
        {
            string typeDeclaration = typeSymbol.ToDisplayString(new SymbolDisplayFormat(
                SymbolDisplayGlobalNamespaceStyle.Included,
                SymbolDisplayTypeQualificationStyle.NameOnly,
                SymbolDisplayGenericsOptions.IncludeTypeParameters,
                SymbolDisplayMemberOptions.IncludeModifiers,
                kindOptions: SymbolDisplayKindOptions.IncludeTypeKeyword));

            sourceWriter.WriteLine("partial " + typeDeclaration);
        }

        private void GenerateFields(IndentedTextWriter sourceWriter)
        {
            foreach (var @interface in _interfaces)
            {
                sourceWriter.Write("private");
                sourceWriter.Write(" ");
                sourceWriter.Write(@interface.TypeSymbol.ToDisplayString(SymbolStyles.FullTypeName));
                sourceWriter.Write(" ");
                sourceWriter.Write(@interface.FieldName);
                sourceWriter.WriteLine(";");
            }

            sourceWriter.WriteLine();
        }

        internal class InterfaceData
        {
            public INamedTypeSymbol TypeSymbol { get; }

            public string FieldName { get; }
            
            public InterfaceData(INamedTypeSymbol typeSymbol)
            {
                TypeSymbol = typeSymbol;
                FieldName = typeSymbol.Name + "_" + Guid.NewGuid().ToString().Replace("-", "");
            }
        }
    }
}