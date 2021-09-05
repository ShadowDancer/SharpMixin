using Microsoft.CodeAnalysis;
using SharpMixin.Attributes;

namespace SharpMixin.Generator
{
    public static class Diagnostics
    {
        public static readonly DiagnosticDescriptor NonPartialMixin = new(
            "MIX0001",
            "Non partial Mixin",
            "{0} is not partial. Mixin classes must be partial in order to generate code",
            "Mixin",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor MixinInNonPartialClass = new(
            "MIX0002",
            "Mixin in non partial class",
            "{0} is nested type of {1} which is not partial. If Mixin is inside nested class, owning classes must be partial to generate code.",
            "Mixin",
            DiagnosticSeverity.Error,
            true);


        public static readonly DiagnosticDescriptor MixinWithoutInterfaces = new(
            "MIX0100",
            "Mixin does not implement interfaces",
            "{0} is marked as mixin but does not implement any interfaces. Mixins are generated for declared interfaces.",
            "Mixin",
            DiagnosticSeverity.Warning,
            true);

        public static class ConstructUsing
        {
            public static readonly DiagnosticDescriptor EmptyConstructor = new(
                "MIX0200",
                $"{nameof(ConstructUsingAttribute)} does not have any type parameters.",
                "Empty construct using attribute does nothing, and can be removed.",
                "Mixin",
                DiagnosticSeverity.Warning,
                true);


            public static readonly DiagnosticDescriptor CannotParseType = new(
                "MIX0201",
                "Cannot parse constructor parameter of " + nameof(ConstructUsingAttribute),
                "Cannot parse \"{0}\" as type. Only typeof expressions are supported.",
                "Mixin",
                DiagnosticSeverity.Warning,
                true);

            public static readonly DiagnosticDescriptor UnusedInterface = new(
                "MIX0202",
                "Unused type in constructor",
                "Type {0} is passed to constructor of {1} but is not required to create mapping. " +
                "Either too many types were passed to " + nameof(ConstructUsingAttribute) +
                " or no conversion exists to interface implemented by mixin.",
                "Mixin",
                DiagnosticSeverity.Warning,
                true);
        }
    }
}