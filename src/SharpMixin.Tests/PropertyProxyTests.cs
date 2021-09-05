using System;
using System.Collections.Generic;
using NSubstitute;
using SharpMixin.Attributes;
using Xunit;

namespace SharpMixin.Tests
{
    public partial class PropertyProxyTests
    {
        [Fact]
        public void Should_ProxyGet_For_SimpleProperty()
        {
            string expected = Guid.NewGuid().ToString();
            var implementation = Substitute.For<IPropertyInterface<string>>();
            implementation.SimpleProp.Returns(expected);

            var mixin = new PropertyMixin<string>(implementation);
            var actual = mixin.SimpleProp;
            
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void Should_ProxyGet_For_ComplexProperty()
        {
            NestedList<string> expected = new NestedList<string>();
            var implementation = Substitute.For<IPropertyInterface<string>>();
            implementation.GenericProp.Returns(expected);

            var mixin = new PropertyMixin<string>(implementation);
            var actual = mixin.GenericProp;
            
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void Should_ProxySet_For_SimpleProperty()
        {
            List<string> receivedCalls = new();
            var implementation = Substitute.For<IPropertyInterface<string>>();
            implementation.SimpleProp = Arg.Do<string>(arg => receivedCalls.Add(arg));
            
            string str = Guid.NewGuid().ToString();
            _ = new PropertyMixin<string>(implementation)
            {
                SimpleProp = str
            };
            
            Assert.Single(receivedCalls);
            Assert.Same(str, receivedCalls[0]);
        }
        
        [Fact]
        public void Should_ProxySet_For_GenericProperty()
        {
            List<NestedList<string>> receivedCalls = new();
            var implementation = Substitute.For<IPropertyInterface<string>>();
            implementation.GenericProp = Arg.Do<NestedList<string>>(arg => receivedCalls.Add(arg));
            
            NestedList<string> list = new NestedList<string>();
            _ = new PropertyMixin<string>(implementation)
            {
                GenericProp = list
            };
            
            Assert.Single(receivedCalls);
            Assert.Same(list, receivedCalls[0]);
        }
        
        public interface IPropertyInterface<T>
        {
            public string SimpleProp { get; set; }
            public NestedList<T> GenericProp { get; set; }
        }

        public class NestedList<T> : List<T>
        {
        }


        [Mixin]
        public partial class PropertyMixin<T> : IPropertyInterface<T>
        {
        }
    }
}