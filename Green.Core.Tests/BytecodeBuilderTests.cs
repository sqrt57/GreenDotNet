using Xunit;

namespace Green.Tests
{
    public class BytecodeBuilderTests
    {
        [Fact]
        public void AddConstant()
        {
            var builder = new BytecodeBuilder();
            var index1 = builder.AddConstant(10);
            var index2 = builder.AddConstant(20);
            var result = builder.ToBytecode();

            Assert.Equal(0, index1);
            Assert.Equal(1, index2);
            Assert.Equal(new object[] { 10, 20 }, result.Constants);
        }
    }
}
