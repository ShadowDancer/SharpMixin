using System.Collections.Immutable;

namespace SharpMixin.Generator.Configuration
{
    public class MixinConfiguration
    {
        public bool GenerateDefaultConstructor { get; }

        public MixinConfiguration(bool generateDefaultConstructor,
            IImmutableList<ConstructorConfiguration> additionalConstructors)
        {
            GenerateDefaultConstructor = generateDefaultConstructor;
            AdditionalConstructors = additionalConstructors;
        }

        public IImmutableList<ConstructorConfiguration> AdditionalConstructors { get; }
    }
}