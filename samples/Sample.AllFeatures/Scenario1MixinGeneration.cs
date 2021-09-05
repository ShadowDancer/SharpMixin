using System;
using SharpMixin.Attributes;
using Xunit;
// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace Sample.AllFeatures
{
    public interface IWithProperty
    {
        public string? Property { get; set; }
    }

    public interface IWithMethod
    {
        public string Method();
    }

    /// <summary>
    /// This is mixin class (designated by mixin attribute, and partial modifier).
    /// It implements two interfaces. Notice there is no implementation of these interfaces in the class.
    /// </summary>
    [Mixin]
    public partial class SimpleMixin : IWithMethod, IWithProperty
    {
        // You can see generated code in GeneratedFiles directory after build
    }

    public class Scenario1MixinGeneration
    {
        [Fact]
        public void MixinShowcase()
        {
            WithMethod withMethodImplementation = new WithMethod()
            {
                MethodImpl = () => "Hello "
            };
            WithProperty withPropertyImplementation = new WithProperty()
            {
                Property = "World"
            };

            // Constructor is automatically implemented by library
            SimpleMixin mixin = new SimpleMixin(withMethodImplementation, withPropertyImplementation);

            // all method calls are proxied to underlying implementations
            string helloWorld = mixin.Method() + mixin.Property;

            Assert.Equal("Hello World", helloWorld);
        }
    }

    public class WithMethod : IWithMethod
    {
        public Func<string>? MethodImpl { get; set; }

        public string Method()
        {
            return MethodImpl!();
        }
    }

    public class WithProperty : IWithProperty
    {
        public string? Property { get; set; }
    }

}