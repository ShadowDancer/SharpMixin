using SharpMixin.Attributes;
using Xunit;

namespace Sample.AllFeatures
{
    /// <summary>
    ///     Imagine you have class which implements IWithMethod, IWithProperty,
    ///     and you want to add third interface to it.
    ///     You can create custom constructor which will supply this class to both these interfaces.
    /// </summary>
    [Mixin]
    [ConstructUsing(typeof(SimpleMixin))]
    public partial class ExpandedMixin : IWithMethod, IWithProperty, IAdditional
    {
    }

    public interface IAdditional
    {
        public string Additional => "!!!";
    }

    public class Scenario2CustomConstructors
    {
        [Fact]
        public void Scenario2()
        {
            SimpleMixin? simpleMixin = new SimpleMixin(new WithMethod
            {
                MethodImpl = () => "Hello "
            }, new WithProperty
            {
                Property = "World"
            });

            // All calls to IWithMethod, IWithProperty will be proxied to SimpleMixin
            // Calls to IAdditional will be proxied to second constructor argument
            // You can use this approach when your mixin methods are complex and implement many interfaces
            ExpandedMixin? expandedMixin = new ExpandedMixin(simpleMixin, new AdditionalImpl());

            Assert.Equal("Hello World!!!", expandedMixin.Method() + expandedMixin.Property + expandedMixin.Additional);
        }
    }

    public class AdditionalImpl : IAdditional
    {
    }
}