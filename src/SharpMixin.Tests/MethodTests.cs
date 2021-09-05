using System.Collections.Generic;
using AutoFixture;
using NSubstitute;
using SharpMixin.Attributes;
using Xunit;
using MethodInterface = SharpMixin.Tests.MethodTests.IMethodInterface<string>;

namespace SharpMixin.Tests
{
    public partial class MethodTests
    {
        private readonly IMethodInterface<string> _mock;
        private readonly MethodMixin<string> _testObj;
        private Fixture _fixture = new();

        public MethodTests()
        {
            _mock = Substitute.For<MethodInterface>();
            _testObj = new MethodMixin<string>(_mock);
        }

        [Fact]
        public void Should_ProxyCall_For_VoidMethod()
        {
            _testObj.VoidMethod();
            
            _mock.Received(1).VoidMethod();
        }
        
        
        [Fact]
        public void Should_ProxyCall_For_NonVoidMethod()
        {
            string expectedReturnValue = _fixture.Create<string>();
            _mock.NonVoidMethod().Returns(expectedReturnValue);
            
            string actual = _testObj.NonVoidMethod();
            
            Assert.Equal(expectedReturnValue, actual);
        }
        
        [Fact]
        public void Should_ProxyCall_For_GenericMethod()
        {
            string expectedReturnValue = _fixture.Create<string>();
            _mock.GenericMethod<string>().Returns(expectedReturnValue);
            
            string actual = _testObj.GenericMethod<string>();
            
            Assert.Equal(expectedReturnValue, actual);
        }
        
        [Fact]
        public void Should_ProxyCall_For_MethodWithArg()
        {
            string param = _fixture.Create<string>();
            
            _testObj.MethodWithArg(param);
            
            _mock.Received(1).MethodWithArg(param);
        }
        
        [Fact]
        public void Should_ProxyCall_For_MethodWithMultipleArgs()
        {
            string param1 = _fixture.Create<string>();
            object param2 = _fixture.Create<object>();
            bool param3 = _fixture.Create<bool>();
            
            _testObj.MethodWithMultipleArgs(param1, param2, param3);
            
            _mock.Received(1).MethodWithMultipleArgs(param1, param2, param3);
        }

        [Fact]
        public void Should_ProxyCall_For_MethodWithGenericArg()
        {
            var param = _fixture.Create<List<string>>();
            
            _testObj.MethodWithGenericArg(param);
            
            _mock.Received(1).MethodWithGenericArg(param);
        }
        
        [Fact]
        public void Should_ProxyCall_For_MethodWithOpenGenericArg()
        {
            var param = _fixture.Create<List<string>>();
            
            _testObj.MethodWithOpenGenericArg(param);
            
            _mock.Received(1).MethodWithOpenGenericArg(param);
        }

        
        
        public interface IMethodInterface<T>
        {
            public void VoidMethod();
            public string NonVoidMethod();
            public List<string> GenericMethodReturn();
            public TZ GenericMethod<TZ>();
            public void MethodWithArg(string arg);
            public void MethodWithMultipleArgs(string arg1, object arg2, bool arg3);
            public void MethodWithGenericArg(List<string>  arg);
            public void MethodWithOpenGenericArg(List<T>  arg);
        }

        [Mixin]
        public partial class MethodMixin<T> : IMethodInterface<T>
        {
        }
    }
}
