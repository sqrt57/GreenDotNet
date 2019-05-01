using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Green.Tests
{
    public class SourcePositionBuilderTests
    {
        [Fact]
        public void SourcePositionBuilder_Initial_ZeroPosition()
        {
            var builder = new SourcePositionBuilder();
            Assert.Equal(new SourcePosition(0, 0, 0), builder.Current);
        }

        [Fact]
        public void SourcePositionBuilder_OneLine()
        {
            var builder = new SourcePositionBuilder();
            builder.Update('q');
            builder.Update('w');
            Assert.Equal(new SourcePosition(2, 0, 2), builder.Current);
        }

        [Fact]
        public void SourcePositionBuilder_SecondLine()
        {
            var builder = new SourcePositionBuilder();
            builder.Update('q');
            builder.Update('\n');
            builder.Update('w');
            Assert.Equal(new SourcePosition(3, 1, 1), builder.Current);
        }

        [Fact]
        public void SourcePositionBuilder_TwoCurrents_AreIndependent()
        {
            var builder = new SourcePositionBuilder();
            var position1 = builder.Current;
            builder.Update('q');
            var position2 = builder.Current;
            Assert.NotEqual(position1, position2);
        }
    }
}
