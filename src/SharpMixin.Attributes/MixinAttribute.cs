using System;

namespace SharpMixin.Attributes
{
    /// <summary>
    /// Marks class as mixin. Source generator will create constructor and proxy for interface calls,
    /// so this object may act as mix of all declared interfaces.
    /// All calls to members of these interfaces will be proxied to objects passed to constructor.
    ///
    /// Classes declared as mixins should be public and interfaces implemented.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class MixinAttribute : Attribute
    {
    }
}