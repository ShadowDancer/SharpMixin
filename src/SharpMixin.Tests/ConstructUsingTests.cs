using AutoFixture;
using NSubstitute;
using SharpMixin.Attributes;
using Xunit;

namespace SharpMixin.Tests
{
    public partial class ConstructUsingTests
    {
        private readonly Fixture _fixture = new();
        
        [Fact]
        public void Should_ConstructUsing_Generate_AdditionalConstructor()
        {
            var implementationAb = new ImplementationAb()
            {
                A = _fixture.Create<string>(),
                B = _fixture.Create<string>()
            };
            var implementationC = Substitute.For<IInterfaceC>();
            var c = _fixture.Create<string>();
            implementationC.C.Returns(c);

            var testObj = new ConstructUsingMixin(implementationAb, implementationC);
            
            Assert.Equal(implementationAb.A, testObj.A);
            Assert.Equal(implementationAb.B, testObj.B);
            Assert.Equal(implementationC.C, testObj.C);
        }

        [Mixin]
        [ConstructUsing(typeof(ImplementationAb))]
        public partial class ConstructUsingMixin : IInterfaceA, IInterfaceB, IInterfaceC
        {
        }

        public class ImplementationAb : IInterfaceA, IInterfaceB
        {
            public string A { get; set; }
            public string B { get; set; }
        }
        
        public interface IInterfaceA
        {
            public string A { get; set; }
        }
        public interface IInterfaceB
        {
            public string B { get; set; }
        }
        public interface IInterfaceC
        {
            public string C { get; set; }
        }
    }
}
