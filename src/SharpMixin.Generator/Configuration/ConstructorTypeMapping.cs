using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SharpMixin.Generator.Configuration
{
    public class ConstructorTypeMapping
    {
        public ConstructorTypeMapping(INamedTypeSymbol typeSymbol, IImmutableList<INamedTypeSymbol> destinationSymbols)
        {
            TypeSymbol = typeSymbol;
            DestinationSymbols = destinationSymbols;
        }

        public INamedTypeSymbol TypeSymbol { get; }

        public IImmutableList<INamedTypeSymbol> DestinationSymbols { get; }
    }
}