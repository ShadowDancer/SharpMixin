using System.CodeDom.Compiler;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SharpMixin.Generator.Generator.Utils;

namespace SharpMixin.Generator.Generator
{
    internal static class InterfaceProxyGenerator
    {
        internal class InterfaceInfo
        {
            public InterfaceInfo(INamedTypeSymbol interfaceSymbol, string fieldName)
            {
                InterfaceSymbol = interfaceSymbol;
                FieldName = fieldName; //interfaceSymbol.Name + "_" + Guid.NewGuid().ToString().Replace("-", "");
                FullTypeName = interfaceSymbol.ToDisplayString(SymbolStyles.FullTypeName);
            }

            public INamedTypeSymbol InterfaceSymbol { get; }

            public string FullTypeName { get; }
        
            public string FieldName { get; }
        }
        
        public static void GenerateInterfaceProxies(IndentedTextWriter sourceWriter, IImmutableList<InterfaceInfo> interfaces)
        {
            foreach (var interfaceInfo in interfaces)
            {
                sourceWriter.Write("#region");
                sourceWriter.Write(" ");
                sourceWriter.WriteLine(interfaceInfo.FullTypeName);
                GenerateInterfaceProxy(sourceWriter, interfaceInfo);

                sourceWriter.WriteLine("#endregion");
                sourceWriter.WriteLine();
            }
        }

        private static void GenerateInterfaceProxy(IndentedTextWriter sourceWriter, InterfaceInfo interfaceInfo)
        {
            var symbol = interfaceInfo.InterfaceSymbol;
            foreach (var member in symbol.GetMembers())
            {
                switch (member)
                {
                    case IPropertySymbol propertySymbol:
                        GeneratePropertyProxy(sourceWriter, interfaceInfo, propertySymbol);
                        break;
                    case IMethodSymbol methodSymbol:
                        GenerateMethodProxy(sourceWriter, interfaceInfo, methodSymbol);
                        break;
                }
            }
        }

        private static void GeneratePropertyProxy(IndentedTextWriter sourceWriter, InterfaceInfo interfaceInfo,
            IPropertySymbol propertySymbol)
        {
            sourceWriter.Write(AccessibilityToDeclaration(propertySymbol.DeclaredAccessibility));
            sourceWriter.Write(" ");
            sourceWriter.Write(propertySymbol.Type.ToDisplayString(SymbolStyles.FullTypeName));
            sourceWriter.Write(" ");
            sourceWriter.Write(propertySymbol.ToDisplayString(SymbolStyles.FullTypeName));
            sourceWriter.WriteLine(" {");

            sourceWriter.Indent++;
            if (!propertySymbol.IsWriteOnly)
            {
                if (propertySymbol.GetMethod!.DeclaredAccessibility != propertySymbol.DeclaredAccessibility)
                {
                    sourceWriter.Write(AccessibilityToDeclaration(propertySymbol.GetMethod.DeclaredAccessibility));
                    sourceWriter.Write(" ");
                }

                sourceWriter.Write("get => ");
                sourceWriter.Write(interfaceInfo.FieldName);
                sourceWriter.Write(".");
                sourceWriter.Write(propertySymbol.Name);
                sourceWriter.WriteLine(";");
            }

            if (!propertySymbol.IsReadOnly)
            {
                if (propertySymbol.SetMethod!.DeclaredAccessibility != propertySymbol.DeclaredAccessibility)
                {
                    sourceWriter.Write(AccessibilityToDeclaration(propertySymbol.SetMethod.DeclaredAccessibility));
                    sourceWriter.Write(" ");
                }

                sourceWriter.Write("set => ");
                sourceWriter.Write(interfaceInfo.FieldName);
                sourceWriter.Write(".");
                sourceWriter.Write(propertySymbol.Name);
                sourceWriter.WriteLine(" = value;");
            }

            sourceWriter.Indent--;

            sourceWriter.WriteLine("}");
        }

        private static void GenerateMethodProxy(IndentedTextWriter sourceWriter, InterfaceInfo interfaceInfo,
            IMethodSymbol methodSymbol)
        {
            if (methodSymbol.MethodKind != MethodKind.Ordinary)
            {
                return;
            }

            sourceWriter.Write(AccessibilityToDeclaration(methodSymbol.DeclaredAccessibility));
            sourceWriter.Write(" ");

            var methodDeclarationFormat = new SymbolDisplayFormat(
                parameterOptions: SymbolDisplayParameterOptions.IncludeType |
                                  SymbolDisplayParameterOptions.IncludeName |
                                  SymbolDisplayParameterOptions.IncludeParamsRefOut |
                                  SymbolDisplayParameterOptions.IncludeDefaultValue,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeVariance |
                                 SymbolDisplayGenericsOptions.IncludeTypeConstraints |
                                 SymbolDisplayGenericsOptions.IncludeTypeParameters,
                memberOptions: SymbolDisplayMemberOptions.IncludeAccessibility |
                               SymbolDisplayMemberOptions.IncludeModifiers |
                               SymbolDisplayMemberOptions.IncludeParameters |
                               SymbolDisplayMemberOptions.IncludeRef |
                               SymbolDisplayMemberOptions.IncludeType,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                                      SymbolDisplayMiscellaneousOptions.AllowDefaultLiteral |
                                      SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                                      SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            );
            sourceWriter.WriteLine(methodSymbol.ToDisplayString(methodDeclarationFormat));
            using (sourceWriter.Scope())
            {
                if (!methodSymbol.ReturnsVoid)
                {
                    sourceWriter.Write("return ");
                }

                sourceWriter.Write(interfaceInfo.FieldName);
                sourceWriter.Write(".");


                var methodCallFormat = new SymbolDisplayFormat(
                    parameterOptions: SymbolDisplayParameterOptions.IncludeName |
                                      SymbolDisplayParameterOptions.IncludeParamsRefOut,
                    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                    memberOptions: SymbolDisplayMemberOptions.IncludeParameters |
                                   SymbolDisplayMemberOptions.IncludeRef
                );
                sourceWriter.Write(methodSymbol.ToDisplayString(methodCallFormat));
                sourceWriter.WriteLine(";");
            }

            sourceWriter.WriteLine();
        }

        private static string AccessibilityToDeclaration(Accessibility accessibility)
        {
            if (accessibility is Accessibility.Internal or Accessibility.Public or Accessibility.Private or
                Accessibility.Protected)
            {
                return accessibility.ToString().ToLowerInvariant();
            }

            if (accessibility is Accessibility.ProtectedAndInternal or Accessibility.ProtectedOrInternal)
            {
                return "protected internal";
            }

            return "unknown";
        }
    }
}