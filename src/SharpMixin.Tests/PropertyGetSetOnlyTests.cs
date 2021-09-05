using SharpMixin.Attributes;
using Xunit;

namespace SharpMixin.Tests
{
    public partial class PropertyGetSetOnlyTests
    {
        [Fact]
        public void Should_GetOnlyProperty_NotHave_Setter()
        {
            var getOnlyProperty = typeof(GetSetOnlyMixin).GetProperty(nameof(IGetOnly.Get));
            
            Assert.NotNull(getOnlyProperty);
            Assert.False(getOnlyProperty.CanWrite);
            Assert.True(getOnlyProperty.CanRead);
        }

        [Fact]
        public void Should_SetOnlyProperty_NotHave_Setter()
        {
            var setOnlyProperty = typeof(GetSetOnlyMixin).GetProperty(nameof(ISetOnly.Set));
            
            Assert.NotNull(setOnlyProperty);
            Assert.True(setOnlyProperty.CanWrite);
            Assert.False(setOnlyProperty.CanRead);
        }
        
        [Mixin]
        public partial class GetSetOnlyMixin : IGetOnly, ISetOnly
        {
            
        } 
        
        public interface IGetOnly
        {
            string Get { get; }
        }
        public interface ISetOnly
        {
            string Set { set; }
        }
    }
}