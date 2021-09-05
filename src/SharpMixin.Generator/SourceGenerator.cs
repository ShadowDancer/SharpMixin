using Microsoft.CodeAnalysis;
using SharpMixin.Generator.Generator;
#if DEBUG
using System.Diagnostics;
#endif

namespace SharpMixin.Generator
{
    [Generator]
    public class SharpMixinSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
#if DEBUG
            Debugger.Launch();
#endif
            new GenerationOrchestrator(context).Generate();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}