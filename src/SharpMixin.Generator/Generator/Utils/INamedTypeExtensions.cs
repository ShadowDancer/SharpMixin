using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SharpMixin.Generator.Generator.Utils
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Returns array containing all parents, starting from closest ending at top-level.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <returns></returns>
        public static ImmutableArray<INamedTypeSymbol> GetContainingTypes(this INamedTypeSymbol typeSymbol)
        {
            INamedTypeSymbol? current = typeSymbol;
            List<INamedTypeSymbol> containingTypes = new();

            while (true)
            {
                INamedTypeSymbol owning = current.ContainingType;
                current = owning;
                if (current == null)
                {
                    break;
                }

                containingTypes.Add(current);
            }

            return containingTypes.ToImmutableArray();
        }
    }
}