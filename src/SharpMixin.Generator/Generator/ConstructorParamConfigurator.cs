using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using SharpMixin.Generator.Configuration;

namespace SharpMixin.Generator.Generator
{
    /// <summary>
    /// Takes list of types and creates constructor configuration from it.
    /// Configured types are assigned to interfaces.
    ///
    /// All interfaces which are not in configuration will be added as simpleConfiguration.
    /// Constructor parameter names will be human readable and will have no collisions.
    /// </summary>
    internal class ConstructorParamConfigurator
    {
        public static IImmutableList<ConstructorGenerator.Param> MapTypesToFields(Compilation compilation,
            IEnumerable<ClassGenerator.InterfaceData> interfaces, ConstructorConfiguration constructorConfiguration)
        {
            Dictionary<INamedTypeSymbol, List<string>> typeToFieldMapping = constructorConfiguration.TypeMappings
                .ToDictionary(n => n.TypeSymbol, _ => new List<string>());
            List<ClassGenerator.InterfaceData> interfacesToConsume = interfaces.ToList();
            foreach (var type in constructorConfiguration.TypeMappings)
            {
                foreach (var interfaceData in interfacesToConsume.ToArray())
                {
                    foreach (INamedTypeSymbol destinationSymbol in type.DestinationSymbols)
                    {
                        CommonConversion conversion =
                            compilation.ClassifyCommonConversion(destinationSymbol, interfaceData.TypeSymbol);
                        if (conversion.IsIdentity)
                        {
                            interfacesToConsume.Remove(interfaceData);
                            typeToFieldMapping[type.TypeSymbol].Add(interfaceData.FieldName);
                        }
                    }
                }
            }

            foreach (var @interface in interfacesToConsume)
            {
                typeToFieldMapping.Add(@interface.TypeSymbol, new List<string>()
                {
                    @interface.FieldName
                });
            }

            return typeToFieldMapping
                .Select(n => MapToConstructorParam(n.Key, n.Value))
                .ToImmutableArray();
        }

        private static ConstructorGenerator.Param MapToConstructorParam(INamedTypeSymbol typeSymbol,
            List<string> fieldNames)
        {
            return new ConstructorGenerator.Param(typeSymbol.Name,
                typeSymbol, fieldNames.ToImmutableArray());
        }
    }
}