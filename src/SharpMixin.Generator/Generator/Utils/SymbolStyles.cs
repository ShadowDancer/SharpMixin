using Microsoft.CodeAnalysis;

namespace SharpMixin.Generator.Generator.Utils
{
    internal static class SymbolStyles
    {
        public static readonly SymbolDisplayFormat FullTypeName = new(
            SymbolDisplayGlobalNamespaceStyle.Included,
            SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier | 
                                  SymbolDisplayMiscellaneousOptions.UseSpecialTypes | 
                                  SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);
    }
}