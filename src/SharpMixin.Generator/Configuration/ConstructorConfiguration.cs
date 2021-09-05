using System.Collections.Immutable;

namespace SharpMixin.Generator.Configuration
{
    /// <summary>
    /// Defines additional constructor with parameters to configure.
    /// </summary>
    public class ConstructorConfiguration
    {
        public static ConstructorConfiguration Empty()
        {
            return new ConstructorConfiguration(ImmutableArray<ConstructorTypeMapping>.Empty);
        }

        public ConstructorConfiguration(IImmutableList<ConstructorTypeMapping> typeMappings)
        {
            TypeMappings = typeMappings;
        }

        public IImmutableList<ConstructorTypeMapping> TypeMappings { get; }
    }
}