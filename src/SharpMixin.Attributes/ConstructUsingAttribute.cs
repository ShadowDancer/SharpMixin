using System;

namespace SharpMixin.Attributes
{
    /// <summary>
    /// Creates constructor from types declared in this attribute.
    /// All interfaces declared on mixin but not implemented by provided types will be added as additional constructor params.
    /// Single object on this list may implement multiple interfaces.
    ///
    /// This method allows chaining of mixins, i.e. take existing mixin implementing multiple interfaces and add new object. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class ConstructUsingAttribute : Attribute
    {
        // ReSharper disable once UnusedParameter.Local
        public ConstructUsingAttribute(params Type[] types)
        {
        }
    }
}